// api.js — Cliente HTTP centralizado de SubastaYa
// La URL base está en UN solo lugar: si cambia el puerto,
// se cambia aquí y se propaga a todo el frontend.

const API_URL = "http://localhost:5205/api";

/**
 * Obtiene el listado paginado de subastas con filtros opcionales.
 *
 * La API devuelve: { pagina, tamanioPagina, totalRegistros, totalPaginas, items }
 * items es un array de SubastaDto con los campos:
 *   id, vendedorId, categoriaId, titulo, descripcion, urlImagen,
 *   precioBase, incrementoMinimo, fechaInicio, fechaFin,
 *   estado, pujaActual (nullable), cantidadPujas
 *
 * @param {Object} filtros - { estado, categoriaId, precioMin, precioMax, orden, pagina, tamanioPagina }
 * @returns {Promise<{items: Array, totalRegistros: number}>}
 */
export async function obtenerSubastas(filtros = {}) {
    const params = new URLSearchParams();

    // El backend acepta el enum como string (ej. "ACTIVA") gracias al JsonStringEnumConverter
    if (filtros.estado)       params.append("estado",       filtros.estado);
    if (filtros.categoriaId)  params.append("categoriaId",  filtros.categoriaId);
    if (filtros.precioMin)    params.append("precioMin",    filtros.precioMin);
    if (filtros.precioMax)    params.append("precioMax",    filtros.precioMax);
    if (filtros.orden)        params.append("orden",        filtros.orden);

    // Paginación: traemos todos para el catálogo (página 1, tamaño grande)
    params.append("pagina",        filtros.pagina        ?? 1);
    params.append("tamanioPagina", filtros.tamanioPagina ?? 50);

    const url = `${API_URL}/subastas?${params}`;

    const respuesta = await fetch(url);

    if (!respuesta.ok) {
        throw new Error(`Error al obtener subastas: HTTP ${respuesta.status}`);
    }

    // La API devuelve el objeto paginado: { items: [...], totalRegistros: N, ... }
    const datos = await respuesta.json();
    return {
        items: datos.items ?? [],
        totalRegistros: datos.totalRegistros ?? 0
    };
}

/**
 * Obtiene el detalle completo de una subasta por ID.
 * @param {number} id
 * @returns {Promise<Object>} SubastaDto
 */
export async function obtenerSubastaPorId(id) {
    const respuesta = await fetch(`${API_URL}/subastas/${id}`);

    if (respuesta.status === 404) throw new Error("Subasta no encontrada.");
    if (!respuesta.ok) throw new Error(`Error al obtener la subasta: HTTP ${respuesta.status}`);

    return respuesta.json();
}

/**
 * Registra una nueva puja en una subasta.
 * Maneja explícitamente los códigos HTTP del backend:
 *   - 409 Conflict   → concurrencia (Optimistic Locking)
 *   - 400 Bad Request → validación de negocio (saldo insuficiente, monto bajo, etc.)
 *
 * @param {number} subastaId
 * @param {number} usuarioId
 * @param {number} monto
 * @returns {Promise<{id: number}>}
 */
export async function realizarPuja(subastaId, usuarioId, monto) {
    const respuesta = await fetch(`${API_URL}/subastas/${subastaId}/pujas`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ usuarioId, monto })
    });

    if (respuesta.status === 409) {
        const cuerpo = await respuesta.json();
        throw { tipo: "CONCURRENCIA", mensaje: cuerpo.error ?? "Otra oferta llegó antes. Actualizá e intentá de nuevo." };
    }
    if (respuesta.status === 400) {
        const cuerpo = await respuesta.json();
        throw { tipo: "NEGOCIO", mensaje: cuerpo.error ?? "No se pudo registrar la puja." };
    }
    if (!respuesta.ok) {
        throw { tipo: "ERROR", mensaje: `Error inesperado: HTTP ${respuesta.status}` };
    }

    return respuesta.json();
}

/**
 * Obtiene el historial de pujas de una subasta.
 * @param {number} subastaId
 * @returns {Promise<Array>} Lista de PujaHistorialDto { seudonimo, monto, fechaHora }
 */
export async function obtenerPujas(subastaId) {
    const respuesta = await fetch(`${API_URL}/subastas/${subastaId}/pujas`);
    if (!respuesta.ok) throw new Error(`Error al obtener pujas: HTTP ${respuesta.status}`);
    return respuesta.json();
}

/**
 * Obtiene la billetera de un usuario.
 * @param {number} usuarioId
 * @returns {Promise<{saldoTotal, saldoRetenido, saldoDisponible}>}
 */
export async function obtenerBilletera(usuarioId) {
    const respuesta = await fetch(`${API_URL}/billeteras/${usuarioId}`);
    if (!respuesta.ok) throw new Error(`Error al obtener billetera: HTTP ${respuesta.status}`);
    return respuesta.json();
}

// =========================================================
// BILLETERA
// =========================================================

/**
 * Acredita saldo en la billetera de un usuario.
 * @param {number} usuarioId
 * @param {number} monto
 * @returns {Promise<Object>} BilleteraDto actualizada
 */
export async function depositarSaldo(usuarioId, monto) {
    const respuesta = await fetch(`${API_URL}/billeteras/${usuarioId}/depositos`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ monto: monto })
    });

    if (respuesta.status === 400) {
        const cuerpo = await respuesta.json();
        throw new Error(cuerpo.error ?? "No se pudo acreditar el saldo.");
    }

    if (respuesta.status === 404) {
        throw new Error("No se encontró la billetera del usuario.");
    }

    if (!respuesta.ok) {
        throw new Error(`Error al acreditar saldo: HTTP ${respuesta.status}`);
    }

    return respuesta.json();
}

/**
 * Obtiene los movimientos de la billetera de un usuario.
 * @param {number} usuarioId
 * @returns {Promise<Array>} Lista de TransaccionDto
 */
export async function obtenerTransaccionesBilletera(usuarioId) {
    const respuesta = await fetch(`${API_URL}/billeteras/${usuarioId}/transacciones`);

    if (respuesta.status === 404) {
        throw new Error("No se encontró la billetera del usuario.");
    }

    if (!respuesta.ok) {
        throw new Error(`Error al obtener movimientos: HTTP ${respuesta.status}`);
    }

    return respuesta.json();
}
