package com.japaneseLearning.service;

import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.nio.charset.StandardCharsets;
import java.util.Formatter;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

/**
 * Service to handle MoMo Payment API integration
 */
@Service
@Slf4j
public class MomoService {

    private static final Logger log = LoggerFactory.getLogger(MomoService.class);

    @Value("${external-api.momo.api-url}")
    private String apiUrl;

    @Value("${external-api.momo.secret-key}")
    private String secretKey;

    @Value("${external-api.momo.access-key}")
    private String accessKey;

    @Value("${external-api.momo.partner-code}")
    private String partnerCode;

    @Value("${external-api.momo.return-url}")
    private String returnUrl;

    @Value("${external-api.momo.notify-url}")
    private String notifyUrl;

    private final RestTemplate restTemplate = new RestTemplate();

    /**
     * Create a payment URL by calling MoMo's Create Order API
     */
    public String createPaymentUrl(Long courseId, Double amount, String userId) {
        String orderId = UUID.randomUUID().toString();
        String requestId = UUID.randomUUID().toString();
        String orderInfo = "Thẻ học khóa học #" + courseId;
        // extraData used to pass metadata back to our system
        String extraData = "userId=" + userId + ";courseId=" + courseId;

        // Raw data format required for MoMo signature (Order of fields matters!)
        String rawHash = "accessKey=" + accessKey +
                        "&amount=" + amount.longValue() +
                        "&extraData=" + extraData +
                        "&ipnUrl=" + notifyUrl +
                        "&orderId=" + orderId +
                        "&orderInfo=" + orderInfo +
                        "&partnerCode=" + partnerCode +
                        "&redirectUrl=" + returnUrl +
                        "&requestId=" + requestId +
                        "&requestType=captureWallet";

        try {
            log.info("Generating MoMo signature for RawHash: {}", rawHash);
            String signature = hmacSha256(rawHash, secretKey);

            Map<String, Object> body = new HashMap<>();
            body.put("partnerCode", partnerCode);
            body.put("partnerName", "Japanese Learning Web");
            body.put("storeId", "JapaneseLearningStore");
            body.put("requestId", requestId);
            body.put("amount", amount.longValue());
            body.put("orderId", orderId);
            body.put("orderInfo", orderInfo);
            body.put("redirectUrl", returnUrl);
            body.put("ipnUrl", notifyUrl);
            body.put("lang", "vi");
            body.put("extraData", extraData);
            body.put("requestType", "captureWallet");
            body.put("signature", signature);

            log.info("Sending request to MoMo API: {}", apiUrl);
            @SuppressWarnings("unchecked")
            Map<String, Object> response = restTemplate.postForObject(apiUrl, body, Map.class);
            
            if (response != null) {
                log.info("MoMo API Response: status={}, message={}", response.get("resultCode"), response.get("message"));
                if (response.containsKey("payUrl")) {
                    return (String) response.get("payUrl");
                }
            }
        } catch (Exception e) {
            log.error("Critical error creating MoMo payment URL", e);
        }
        return null;
    }

    /**
     * HMAC SHA256 signature generator
     */
    private String hmacSha256(String data, String key) throws Exception {
        byte[] byteKey = key.getBytes(StandardCharsets.UTF_8);
        final Mac hmacSha256 = Mac.getInstance("HmacSHA256");
        hmacSha256.init(new SecretKeySpec(byteKey, "HmacSHA256"));
        byte[] macData = hmacSha256.doFinal(data.getBytes(StandardCharsets.UTF_8));
        return toHexString(macData);
    }

    private String toHexString(byte[] bytes) {
        try (java.util.Formatter formatter = new java.util.Formatter()) {
            for (byte b : bytes) {
                formatter.format("%02x", b);
            }
            return formatter.toString();
        }
    }
}
