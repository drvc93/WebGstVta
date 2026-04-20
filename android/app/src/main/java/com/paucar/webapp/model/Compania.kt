package com.paucar.webapp.model

data class Compania(
    val id: Int,
    val codigo: String,
    val nombre: String,
    val colorPrimario: String? = null,
    val logoPath: String? = null
)
