package com.japaneseLearning.controller;

import com.japaneseLearning.service.VoucherService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.HashMap;
import java.util.Map;

@RestController
@RequestMapping("/api/voucher")
public class VoucherController {

    private final VoucherService voucherService;

    public VoucherController(VoucherService voucherService) {
        this.voucherService = voucherService;
    }

    @GetMapping("/apply")
    public ResponseEntity<Map<String, Object>> apply(
            @RequestParam String code,
            @RequestParam(defaultValue = "0") long amount
    ) {
        Map<String, Object> response = new HashMap<>();

        Integer discount = voucherService.resolveDiscountPercent(code);
        if (discount == null) {
            response.put("success", false);
            response.put("message", "Voucher code is invalid or expired.");
            return ResponseEntity.ok(response);
        }

        long finalAmount = voucherService.applyDiscount(amount, discount);
        response.put("success", true);
        response.put("discount", discount);
        response.put("saved", amount - finalAmount);
        response.put("data", finalAmount);
        response.put("message", "Voucher applied successfully.");

        return ResponseEntity.ok(response);
    }
}
