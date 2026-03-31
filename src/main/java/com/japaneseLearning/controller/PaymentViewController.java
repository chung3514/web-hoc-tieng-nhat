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
