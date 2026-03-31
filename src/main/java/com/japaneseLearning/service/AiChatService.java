package com.japaneseLearning.service;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

@Service
public class AiChatService {

    @Value("${external-api.gemini.api-key}")
    private String apiKey;

    private final RestTemplate restTemplate;
    private final ObjectMapper objectMapper;

    public AiChatService(ObjectMapper objectMapper) {
        this.restTemplate = new RestTemplate();
        this.objectMapper = objectMapper;
    }

    public String generateReply(String userMessage) {
        String url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey;

        try {
            HttpHeaders headers = new HttpHeaders();
            headers.setContentType(MediaType.APPLICATION_JSON);

            Map<String, Object> payload = new HashMap<>();

            List<Map<String, Object>> contents = new ArrayList<>();
            Map<String, Object> contentItem = new HashMap<>();

            List<Map<String, Object>> parts = new ArrayList<>();
            Map<String, Object> part = new HashMap<>();

            String prompt = "Ban la giao vien tieng Nhat ao (Sensei) cua trang web hoc tieng Nhat, ban hien lanh va than thien.\n"
                    + "YEU CAU BAT BUOC:\n"
                    + "1. Luon giai thich bang tieng Viet ngan gon, de hieu.\n"
                    + "2. Neu co trich dan tieng Nhat, kem Romaji va nghia tieng Viet.\n\n"
                    + "Cau hoi cua hoc vien: " + userMessage;
            part.put("text", prompt);
            parts.add(part);

            contentItem.put("role", "user");
            contentItem.put("parts", parts);
            contents.add(contentItem);

            payload.put("contents", contents);

            HttpEntity<Map<String, Object>> entity = new HttpEntity<>(payload, headers);
            String response = restTemplate.postForObject(url, entity, String.class);

            JsonNode rootNode = objectMapper.readTree(response);
            if (rootNode.has("candidates") && rootNode.get("candidates").isArray() && rootNode.get("candidates").size() > 0) {
                JsonNode candidate = rootNode.get("candidates").get(0);
                if (candidate.has("content") && candidate.get("content").has("parts")) {
                    return candidate.get("content").get("parts").get(0).get("text").asText();
                }
            }

            return "Xin loi, Sensei dang gap loi tam thoi. Em thu lai sau nhe!";
        } catch (Exception ex) {
            return "Loi ket noi AI: " + ex.getMessage();
        }
    }
}
