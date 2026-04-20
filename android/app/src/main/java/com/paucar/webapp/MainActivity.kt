package com.paucar.webapp

import android.os.Bundle
import android.util.Log
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.paucar.webapp.model.LoginRequest
import com.paucar.webapp.network.AuthService
import com.paucar.webapp.network.RetrofitClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class MainActivity : AppCompatActivity() {
    private val loggerTag = "LoginDebug"
    private lateinit var authService: AuthService

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // Initialize Retrofit service
        authService = RetrofitClient.instance.create(AuthService::class.java)
        Log.d(loggerTag, "Retrofit client initialized with base URL: ${RetrofitClient.BASE_URL}")

        val btnLogin = findViewById<Button>(R.id.btn_login)
        val etUsername = findViewById<EditText>(R.id.et_username)
        val etPassword = findViewById<EditText>(R.id.et_password)

        btnLogin.setOnClickListener {
            val username = etUsername.text.toString()
            val password = etPassword.text.toString()
            Log.d(loggerTag, "Login button clicked. Username: $username")

            val request = LoginRequest(username, password)
            Log.d(loggerTag, "Request URL: ${RetrofitClient.BASE_URL}api/auth/login")
            Log.d(loggerTag, "Login request payload: $request")
            authService.login(request).enqueue(object : Callback<com.paucar.webapp.model.LoginResponse> {
                override fun onResponse(call: Call<com.paucar.webapp.model.LoginResponse>, response: Response<com.paucar.webapp.model.LoginResponse>) {
                    if (response.isSuccessful) {
                        val body = response.body()
                        if (body == null) {
                            Log.e(loggerTag, "Login response body is null")
                            Toast.makeText(this@MainActivity, "Respuesta vacia del servidor", Toast.LENGTH_SHORT).show()
                            return
                        }

                        if (body.requiresCompaniaSelection) {
                            Log.d(
                                loggerTag,
                                "Login requiere seleccion de compania. Usuario=${body.username}, companias=${body.companiasDisponibles.size}"
                            )
                            Toast.makeText(
                                this@MainActivity,
                                "Seleccione una compania para continuar",
                                Toast.LENGTH_SHORT
                            ).show()
                        } else {
                            Log.d(
                                loggerTag,
                                "Login OK. Usuario=${body.username}, tokenType=${body.tokenType}, companiaId=${body.companiaId}"
                            )
                        }
                    } else {
                        Log.e(loggerTag, "Login failed with code: ${response.code()}, message: ${response.message()}")
                    }
                }

                override fun onFailure(call: Call<com.paucar.webapp.model.LoginResponse>, t: Throwable) {
                    Log.e(loggerTag, "Login request error: ${t.localizedMessage}", t)
                }
            })
        }
    }
}

