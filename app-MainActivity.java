package com.zhengshengyou.app;

import android.app.Activity;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.View;
import android.view.WindowManager;

import com.zhengshengyou.engine.Lang;

import java.util.Locale;

public class MainActivity extends Activity {

    private TableView table;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SharedPreferences prefs = getSharedPreferences("settings", MODE_PRIVATE);
        String saved = prefs.getString("lang", null);
        Lang.setCode(saved != null ? saved : detectLanguage());

        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        table = new TableView(this);
        setContentView(table);
        hideSystemUi();
        table.startNewMatch();
    }

    private String detectLanguage() {
        String code = Locale.getDefault().getLanguage();
        String country = Locale.getDefault().getCountry();
        if ("zh".equals(code)) return ("TW".equals(country) || "HK".equals(country) || "MO".equals(country)) ? "zh-TW" : "zh-CN";
        if ("fr".equals(code)) return "fr";
        if ("ru".equals(code)) return "ru";
        if ("ja".equals(code)) return "ja";
        if ("es".equals(code)) return "es";
        if ("ar".equals(code)) return "ar";
        return "en";
    }

    public void saveLanguage(String code) {
        Lang.setCode(code);
        getSharedPreferences("settings", MODE_PRIVATE).edit().putString("lang", code).commit();
    }

    @Override
    public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        if (hasFocus) hideSystemUi();
    }

    private void hideSystemUi() {
        View decor = getWindow().getDecorView();
        decor.setSystemUiVisibility(View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_FULLSCREEN
                | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
    }

    @Override
    public void onBackPressed() {
        table.confirmExit();
    }

    @Override
    protected void onDestroy() {
        table.stopTimers();
        super.onDestroy();
    }
}
