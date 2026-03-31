package com.japaneseLearning.controller;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;

@Controller
@RequestMapping("/payment")
public class PaymentViewController {

    @GetMapping("/checkout")
    public String checkout(Model model) {
        model.addAttribute("courseName", "N5 Full Course");
        model.addAttribute("amount", 1299000L);
        return "payment/checkout";
    }

    @GetMapping("/checkout-qr")
    public String checkoutQr(
            @RequestParam(defaultValue = "") String orderId,
            @RequestParam(defaultValue = "") String method,
            @RequestParam(defaultValue = "0") long amount,
            Model model
    ) {
        String resolvedOrderId = orderId == null || orderId.isBlank() ? "ORD-" + System.currentTimeMillis() : orderId;
        String resolvedMethod = method == null || method.isBlank() ? "MOMO" : method;
        long resolvedAmount = amount <= 0 ? 1299000L : amount;

        model.addAttribute("orderId", resolvedOrderId);
        model.addAttribute("method", resolvedMethod);
        model.addAttribute("amount", resolvedAmount);
        return "payment/checkout-qr";
    }

    @GetMapping("/checkout-success")
    public String checkoutSuccess(
            @RequestParam(defaultValue = "") String orderId,
            @RequestParam(defaultValue = "") String message,
            Model model
    ) {
        model.addAttribute("orderId", orderId == null || orderId.isBlank() ? "ORD-" + System.currentTimeMillis() : orderId);
        model.addAttribute("message", message == null || message.isBlank() ? "Giao dich da duoc thanh toan thanh cong." : message);
        return "payment/checkout-success";
    }

    @GetMapping("/result")
    public String result(
            @RequestParam(defaultValue = "false") boolean success,
            @RequestParam(defaultValue = "") String orderId,
            @RequestParam(defaultValue = "") String message,
            Model model
    ) {
        model.addAttribute("success", success);
        model.addAttribute("orderId", orderId);
        model.addAttribute("message", message);
        return "payment/result";
    }
}
