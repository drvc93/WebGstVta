package com.paucar.webapp.model

data class LoginResponse(
    val requiresCompaniaSelection: Boolean = false,
    val companiasDisponibles: List<Compania> = emptyList(),
    val accessToken: String? = null,
    val expiresAt: String? = null,
    val tokenType: String? = null,
    val username: String? = null,
    val nombreMostrar: String? = null,
    val roles: List<String>? = null,
    val companiaId: Int? = null,
    val companiaNombre: String? = null,
    val colorPrimario: String? = null
)
