package com.paucar.webapp.network

import com.paucar.webapp.model.Compania
import retrofit2.http.*
import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query
import okhttp3.MultipartBody
import okhttp3.RequestBody

interface CompaniaService {
    @GET("api/companias")
    fun getAll(): Call<List<Compania>>

    @GET("api/companias/{id}")
    fun getById(@Path("id") id: Int): Call<Compania>

    @POST("api/companias")
    fun create(@Body compania: Compania): Call<Compania>

    @PUT("api/companias/{id}")
    fun update(@Path("id") id: Int, @Body compania: Compania): Call<Void>

    @DELETE("api/companias/{id}")
    fun delete(@Path("id") id: Int): Call<Void>

    // Upload logo (multipart)
    @Multipart
    @POST("api/companias/logo")
    fun uploadLogo(@Part file: MultipartBody.Part): Call<LogoResponse>

    @DELETE("api/companias/logo")
    fun deleteLogo(@Query("path") path: String): Call<Void>
}

/** Response for logo upload containing the web‑accessible path */
 data class LogoResponse(val path: String)
