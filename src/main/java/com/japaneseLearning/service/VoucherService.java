package com.japaneseLearning.service;

import org.springframework.stereotype.Service;

import java.util.HashMap;
import java.util.Map;

@Service
public class VoucherService {

    private static final Map<String, Integer> DEMO_VOUCHERS = new HashMap<>();

    static {
        DEMO_VOUCHERS.put("WELCOME20", 20);
        DEMO_VOUCHERS.put("JLPT30", 30);
        DEMO_VOUCHERS.put("DEMO50", 50);
    }

    public Integer resolveDiscountPercent(String code) {
        if (code == null) {
            return null;
        }
        return DEMO_VOUCHERS.get(code.trim().toUpperCase());
    }

    public long applyDiscount(long amount, int percent) {
        if (amount <= 0 || percent <= 0) {
            return Math.max(amount, 0);
        }
        if (percent >= 100) {
            return 0;
        }
        return Math.round(amount * (100 - percent) / 100.0);
    }
}
