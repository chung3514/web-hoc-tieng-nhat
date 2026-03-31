package com.japaneseLearning.controller;

import com.japaneseLearning.dto.ApiResponse;
import com.japaneseLearning.dto.PaymentRequestDTO;
import com.japaneseLearning.entity.Payment;
import com.japaneseLearning.service.PaymentService;
import com.japaneseLearning.service.VoucherService;
import com.japaneseLearning.service.MomoService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.util.HashMap;
import java.util.Map;

@RestController
@RequestMapping("/api/payment")
@CrossOrigin(origins = "*", maxAge = 3600)
public class PaymentController {
    
    private final PaymentService paymentService;
    private final VoucherService voucherService;
    private final MomoService momoService;

    public PaymentController(PaymentService paymentService, 
                             VoucherService voucherService,
                             MomoService momoService) {
        this.paymentService = paymentService;
        this.voucherService = voucherService;
        this.momoService = momoService;
    }

    @PostMapping("/momo")
    public ResponseEntity<ApiResponse<Map<String, Object>>> createMomoPayment(
            @RequestBody PaymentRequestDTO request) {
        
        org.springframework.security.core.Authentication auth = 
            org.springframework.security.core.context.SecurityContextHolder.getContext().getAuthentication();
        String userId = auth != null ? auth.getName() : "anonymous";
        
        Double finalAmount = request.getAmount();
        
        // Apply voucher if provided
        if (request.getVoucherCode() != null && !request.getVoucherCode().isEmpty()) {
            finalAmount = voucherService.applyVoucher(request.getVoucherCode(), request.getAmount());
        }

        // 1. Create local payment record
        Payment payment = paymentService.createPayment(
                userId,
                request.getCourseId(),
                finalAmount,
                "MOMO"
        );

        // 2. Call MoMo API to get payUrl
        String payUrl = momoService.createPaymentUrl(request.getCourseId(), finalAmount, userId);

        Map<String, Object> response = new HashMap<>();
        response.put("paymentId", payment.getPaymentId());
        response.put("amount", finalAmount);
        response.put("status", "PENDING");
        response.put("payUrl", payUrl); // Return this for frontend redirect

        if (payUrl == null) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body(ApiResponse.error("Failed to initiate MoMo payment"));
        }

        return ResponseEntity.status(HttpStatus.CREATED)
                .body(ApiResponse.success(response, "MoMo payment initiated successfully"));
    }

    /**
     * Mock payment for quick demonstration/testing
     */
    @PostMapping("/mock")
    public ResponseEntity<ApiResponse<Map<String, Object>>> createMockPayment(
            @RequestBody PaymentRequestDTO request) {
        
        org.springframework.security.core.Authentication auth = 
            org.springframework.security.core.context.SecurityContextHolder.getContext().getAuthentication();
        String userId = auth != null ? auth.getName() : "anonymous";
        
        // 1. Create payment
        Payment payment = paymentService.createPayment(
                userId,
                request.getCourseId(),
                request.getAmount(),
                "MOCK_PAYMENT"
        );

        // 2. Instantly complete it for mock flow
        paymentService.updatePaymentStatus(payment.getPaymentId(), Payment.PaymentStatus.COMPLETED);

        Map<String, Object> response = new HashMap<>();
        response.put("status", "SUCCESS");
        response.put("message", "Đã thanh toán thành công (MOCK)");

        return ResponseEntity.ok(ApiResponse.success(response, "Thanh toán thành công. Bạn đã có thể vào học!"));
    }

    @PostMapping("/callback")
    public ResponseEntity<ApiResponse<Void>> paymentCallback(@RequestParam Map<String, String> params) {
        // Handle MoMo payment callback
        String transactionId = params.get("transId");
        String resultCode = params.get("resultCode");

        if ("0".equals(resultCode)) {
            Payment payment = paymentService.getPaymentByTransactionId(transactionId);
            if (payment != null) {
                paymentService.updatePaymentStatus(payment.getPaymentId(), Payment.PaymentStatus.COMPLETED);
            }
            return ResponseEntity.ok(ApiResponse.success(null, "Payment completed successfully"));
        } else {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST)
                    .body(ApiResponse.error("Payment failed"));
        }
    }
}
