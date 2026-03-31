package com.japaneseLearning.controller;

import com.japaneseLearning.service.AiChatService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.HashMap;
import java.util.Map;

@RestController
@RequestMapping("/api/chat")
public class ChatApiController {

    private final AiChatService aiChatService;

    public ChatApiController(AiChatService aiChatService) {
        this.aiChatService = aiChatService;
    }

    @PostMapping
    public ResponseEntity<Map<String, String>> chat(@RequestBody Map<String, String> payload) {
        String userMessage = payload.get("message");

        if (userMessage == null || userMessage.trim().isEmpty()) {
            Map<String, String> err = new HashMap<>();
            err.put("reply", "Loi: Tin nhan khong duoc de trong.");
            return ResponseEntity.badRequest().body(err);
        }

        String reply = aiChatService.generateReply(userMessage);

        Map<String, String> response = new HashMap<>();
        response.put("reply", reply);
        return ResponseEntity.ok(response);
    }
}
