package com.paucar.webapp.network

import com.paucar.webapp.model.LoginRequest
import com.paucar.webapp.model.LoginResponse
import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.Headers
import retrofit2.http.POST

interface AuthService {
    @Headers(
        "Referer: http://38.242.219.173:81/",
        "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36",
        "Accept: application/json, text/plain, */*",
        "Content-Type: application/json"
    )
    @POST("api/auth/login")
    fun login(@Body request: LoginRequest): Call<LoginResponse>
}
