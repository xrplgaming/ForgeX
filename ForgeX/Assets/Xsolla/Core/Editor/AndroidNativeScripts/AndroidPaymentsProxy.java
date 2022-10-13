package com.XsollaInc.XsollaInGameStoreUnityAsset.androidProxies;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

import com.unity3d.player.UnityPlayer;
import com.xsolla.android.payments.XPayments;
import com.xsolla.android.payments.data.AccessToken;

public class AndroidPaymentsProxy extends Activity {

    private static final String ARG_TOKEN = "token";
    private static final String ARG_SANDBOX = "isSandbox";
    private static final String ARG_REDIRECT_HOST = "redirect_host";
    private static final String ARG_REDIRECT_SCHEME = "redirect_scheme";
    private static final int RC_PAY_STATION = 1;

    public static void performPayment(Activity activity, String token, boolean isSandbox, String redirectScheme, String redirectHost) {
        Intent intent = new Intent(activity, AndroidPaymentsProxy.class);
        intent.putExtra(ARG_TOKEN, token);
        intent.putExtra(ARG_SANDBOX, isSandbox);
        intent.putExtra(ARG_REDIRECT_HOST, redirectHost);
        intent.putExtra(ARG_REDIRECT_SCHEME, redirectScheme);
        activity.startActivity(intent);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Intent intent = getIntent();
        String token = intent.getStringExtra(ARG_TOKEN);
        boolean isSandbox = intent.getBooleanExtra(ARG_SANDBOX, false);
        String redirectScheme = intent.getStringExtra(ARG_REDIRECT_SCHEME);
        String redirectHost = intent.getStringExtra(ARG_REDIRECT_HOST);

        XPayments.IntentBuilder builder = XPayments.createIntentBuilder(this)
                .accessToken(new AccessToken(token))
                .isSandbox(isSandbox);

        if (redirectScheme != null)
            builder.setRedirectUriScheme(redirectScheme);

        if (redirectHost != null)
            builder.setRedirectUriHost(redirectHost);

        startActivityForResult(builder.build(), RC_PAY_STATION);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        finish();
    }
}
