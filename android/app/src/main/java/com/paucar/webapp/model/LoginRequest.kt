package com.paucar.webapp.model

/**
 * Data class representing the request body for the login endpoint.
 * Adjust the field names to match the server's expected JSON payload.
 */
data class LoginRequest(
    val username: String,
    val password: String
)
