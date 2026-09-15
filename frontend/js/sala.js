// sala.js — Vista 2: Sala de Subasta en Vivo
// Polling cada 3 segundos, temporizador, anti-sniping,
// manejo de HTTP 201 / 400 / 409 y badge Liderando/Outbid

import { obtenerSubastaPorId, obtenerPujas, realizarPuja } from "./api.js";
import { obtenerUsuarioActivo } from "./app.js";

// ── Estado de la sala ──────────────────────────────────────
let subastaId      = null;   // leído de la URL ?id=N
let estadoActual   = null;   // último SubastaDto recibido
let fechaFinPrevia = null;   // para detectar extensión anti-sniping
let intervaloPoll  = null;   // setInterval del polling (3 s)
let intervaloTimer = null;   // setInterval del countdown (1 s)
let imagenesGaleria = [];// Array de rutas de imágenes para el producto actual
let indiceImagen    = 0;     // Índice de la imagen que se muestra actualmente en la galería

// ── Nodos del DOM ──────────────────────────────────────────
const tituloEl      = document.getElementById("sala-titulo");
const badgeEstadoEl = document.getElementById("sala-badge-estado");
const imgEl         = document.getElementById("sala-img");
const catEl         = document.getElementById("sala-categoria");
// Nodos de la galería de la sala
const thumbsEl      = document.getElementById("sala-thumbs");      // Contenedor de miniaturas
const counterEl     = document.getElementById("galeria-counter");  // Contador numérico (ej. 1/5)
const prevBtn       = document.querySelector(".galeria-arrow--prev"); // Botón flecha izquierda
const nextBtn       = document.querySelector(".galeria-arrow--next"); // Botón flecha derecha

// Panel de puja
const pujaActualEl  = document.getElementById("puja-actual-monto");
const timerEl       = document.getElementById("sala-timer");
const ofertasEl     = document.getElementById("sala-ofertas");
const incMinEl      = document.getElementById("sala-incremento");
const proxPujaEl    = document.getElementById("puja-proxima");
const btnPujaRapEl  = document.getElementById("btn-puja-rapida");
const inputMontoEl  = document.getElementById("input-monto");
const btnRealizarEl = document.getElementById("btn-realizar-puja");
const badgePostorEl = document.getElementById("badge-postor");

// Tabs y descripción
const descEl        = document.getElementById("sala-descripcion");

// Historial
const historialEl   = document.getElementById("historial-body");
const btnVerTodasEl = document.getElementById("btn-ver-todas");
let mostrandoTodas  = false;

// INICIALIZACIÓN — punto de entrada
async function inicializar() {
    // Leer ?id= de la URL
    const params = new URLSearchParams(window.location.search);
    subastaId = parseInt(params.get("id"), 10);

    if (!subastaId || isNaN(subastaId)) {
        mostrarError("URL inválida. No se especificó una subasta.");
        return;
    }

    // Primera carga
    await refrescar();

    // Polling cada 3 segundos (alternativa mínima aceptable según la cátedra)
    intervaloPoll = setInterval(refrescar, 3000);

    // Listeners de la consola de puja
    btnPujaRapEl?.addEventListener("click",    enviarPujaRapida);
    btnRealizarEl?.addEventListener("click",   enviarPujaManual);
    btnVerTodasEl?.addEventListener("click",   toggleVerTodas);
    inputMontoEl?.addEventListener("keydown", e => { if (e.key === "Enter") enviarPujaManual(); });
    prevBtn?.addEventListener("click", () => cambiarImagen(-1));
    nextBtn?.addEventListener("click", () => cambiarImagen(1));

    const favoritoBtn = document.querySelector(".sala-btn-fav");
    favoritoBtn?.addEventListener("click", () => {
        const icono = favoritoBtn.querySelector("i");
        if (!icono) return;
        const activo = icono.classList.contains("fa-solid");
        if (activo) {
            icono.classList.remove("fa-solid");
            icono.classList.add("fa-regular");
            favoritoBtn.classList.remove("favorito-activo");
        } else {
            icono.classList.remove("fa-regular");
            icono.classList.add("fa-solid");
            favoritoBtn.classList.add("favorito-activo");
        }
    });
    // Tabs de descripción
    document.querySelectorAll(".tab-btn").forEach(btn =>
        btn.addEventListener("click", () => cambiarTab(btn)));
}

// REFRESCAR — se llama al inicio y cada 3 s
async function refrescar() {
    try {
        const [subasta, pujas] = await Promise.all([
            obtenerSubastaPorId(subastaId),
            obtenerPujas(subastaId)
        ]);

        // Detectar extensión de tiempo (Anti-Sniping)
        if (fechaFinPrevia && subasta.fechaFin !== fechaFinPrevia) {
            mostrarToast(
                "⏱ Tiempo extendido +2 minutos por regla Anti-Sniping",
                "info"
            );
        }
        fechaFinPrevia = subasta.fechaFin;
        estadoActual   = subasta;

        actualizarUI(subasta, pujas);

    } catch (err) {
        console.error("Error al refrescar la sala:", err);
    }
}
function obtenerImagenProducto(subasta) {

    if (subasta.urlImagen?.startsWith("http")) {
        return subasta.urlImagen;
    }

    const imagenesSeed = {
        "/images/phone.png": "assets/img/celular.jpg",
        "/images/critical.png": "assets/img/notebook.jpg",
        "/images/collectible.png": "assets/img/reloj.webp",
        "/images/jacket.png": "assets/img/zapatillas.jpg",
        "/images/car.png": "assets/img/tv.jpg"
    };

    if (imagenesSeed[subasta.urlImagen]) {
        return imagenesSeed[subasta.urlImagen];
    }

    if (subasta.urlImagen) {
        const nombreArchivo = subasta.urlImagen.split("/").pop();
        return `assets/img/${nombreArchivo}`;
    }

    return "assets/img/notebook.jpg";
}
function inicializarGaleria(s) {
    const imagenPrincipal = obtenerImagenProducto(s);

    // Por ahora, el reloj es el único producto con galería completa.
    if (s.urlImagen?.includes("collectible")) {
        imagenesGaleria = [
            imagenPrincipal,
            "assets/img/reloj2.webp",
            "assets/img/reloj3.webp",
            "assets/img/reloj4.webp",
            "assets/img/reloj5.webp"
        ];
    } else {
        imagenesGaleria = [imagenPrincipal];
    }

    indiceImagen = 0;
    renderizarGaleria();
}
function renderizarGaleria() {

    if (!imgEl || !thumbsEl) return;

    if (imagenesGaleria.length === 0) {
        imagenesGaleria = ["assets/img/notebook.jpg"];
    }

    imgEl.src = imagenesGaleria[indiceImagen];

    if (counterEl) {
        counterEl.textContent =
            `${indiceImagen + 1}/${imagenesGaleria.length}`;
    }

    thumbsEl.innerHTML = imagenesGaleria.map((imagen, indice) => `
        <button
            type="button"
            class="thumb-wrap ${indice === indiceImagen ? "activo" : ""}"
            data-indice="${indice}"
            aria-label="Ver imagen ${indice + 1}"
        >
            <img
                src="${imagen}"
                class="thumb-img"
                alt="Vista ${indice + 1}"
            >
        </button>
    `).join("");

    thumbsEl.querySelectorAll(".thumb-wrap").forEach(btn => {

        btn.addEventListener("click", () => {

            indiceImagen = Number(btn.dataset.indice);

            renderizarGaleria();
        });
    });

    actualizarFlechasGaleria();
}
function actualizarFlechasGaleria() {

    if (!prevBtn || !nextBtn) return;

    const hayVariasImagenes = imagenesGaleria.length > 1;

    prevBtn.disabled = !hayVariasImagenes;
    nextBtn.disabled = !hayVariasImagenes;

    prevBtn.style.opacity = hayVariasImagenes ? "1" : "0.5";
    nextBtn.style.opacity = hayVariasImagenes ? "1" : "0.5";
}
function cambiarImagen(direccion) {

    if (imagenesGaleria.length <= 1) return;

    indiceImagen += direccion;

    if (indiceImagen < 0) {
        indiceImagen = imagenesGaleria.length - 1;
    }

    if (indiceImagen >= imagenesGaleria.length) {
        indiceImagen = 0;
    }

    renderizarGaleria();
}
// ACTUALIZAR UI completa con los datos frescos
function actualizarUI(s, pujas) {
    const CATEGORIAS = { 1:"Tecnología", 2:"Coleccionables", 3:"Indumentaria", 4:"Vehículos" };

    // Cabecera
    if (tituloEl)      tituloEl.textContent      = s.titulo;
    if (catEl)         catEl.textContent          = CATEGORIAS[s.categoriaId] ?? "Categoría";
    if (descEl)        descEl.textContent         = s.descripcion;
    if (imgEl) {
        imgEl.alt = s.titulo;
    }

    if (imagenesGaleria.length === 0) {
        inicializarGaleria(s);
    }

    // Badge de estado
    if (badgeEstadoEl) {
        badgeEstadoEl.textContent  = etiquetaEstado(s.estado);
        badgeEstadoEl.className    = `badge badge--${s.estado}`;
    }

    // Puja actual
    const montoActual = s.pujaActual ?? s.precioBase;
    if (pujaActualEl) pujaActualEl.textContent = moneda(montoActual);
    if (ofertasEl)    ofertasEl.textContent    = `${s.cantidadPujas} oferta${s.cantidadPujas !== 1 ? "s" : ""}`;

    // Próxima puja e incremento mínimo
    const proxMonto = montoActual + s.incrementoMinimo;
    if (incMinEl)    incMinEl.textContent   = moneda(s.incrementoMinimo);
    if (proxPujaEl)  proxPujaEl.textContent = moneda(proxMonto);

    // Botón de puja rápida
    if (btnPujaRapEl) {
        btnPujaRapEl.textContent   = `PUJAR ${moneda(proxMonto)}`;
        btnPujaRapEl.dataset.monto = proxMonto;
        const activa = s.estado === "ACTIVA";
        btnPujaRapEl.disabled      = !activa;
        if (btnRealizarEl) btnRealizarEl.disabled = !activa;
        if (inputMontoEl)  inputMontoEl.disabled  = !activa;
    }

    // Badge de postor (Liderando / Outbid)
    actualizarBadgePostor(s, pujas);

    // Historial de pujas
    renderizarHistorial(pujas);

    // Timer (solo lo (re)inicia si aún no corre)
    if (!intervaloTimer) {
        actualizarTimer(s.fechaFin, s.estado);
        intervaloTimer = setInterval(() => actualizarTimer(estadoActual?.fechaFin, estadoActual?.estado), 1000);
    }
}

// TEMPORIZADOR
function actualizarTimer(fechaFin, estado) {
    if (!timerEl) return;

    // Subasta todavía no comenzó
    if (estado === "PROGRAMADA") {
        const inicio = estadoActual?.fechaInicio;

        if (!inicio) {
            timerEl.textContent = "Próximamente";
            timerEl.className = "sala-timer sala-timer--warn";
            return;
        }

        const restanMs = new Date(inicio).getTime() - Date.now();

        if (restanMs <= 0) {
            timerEl.textContent = "Iniciando...";
            timerEl.className = "sala-timer sala-timer--warn";
            return;
        }

        timerEl.textContent = `Inicia en ${msTiempo(restanMs)}`;
        timerEl.className = "sala-timer sala-timer--warn";
        return;
    }

    // Subasta finalizada o desierta
    if (estado === "FINALIZADA" || estado === "DESIERTA") {
        timerEl.textContent = "Finalizada";
        timerEl.className = "sala-timer sala-timer--inactivo";
        detenerPolling();
        return;
    }

    // Subasta activa
    const restanMs = new Date(fechaFin).getTime() - Date.now();

    if (restanMs <= 0) {
        timerEl.textContent = "00:00:00";
        timerEl.className = "sala-timer sala-timer--critico";
        return;
    }

    timerEl.textContent = msTiempo(restanMs);

    if (restanMs < 60_000) {
        timerEl.className = "sala-timer sala-timer--critico";
    }
    else if (restanMs < 120_000) {
        timerEl.className = "sala-timer sala-timer--warn";
    }
    else {
        timerEl.className = "sala-timer";
    }
}

// BADGE POSTOR: Liderando / Outbid / Sin participar
function actualizarBadgePostor(s, pujas) {

    if (!badgePostorEl) return;

    const usuario = obtenerUsuarioActivo();

    if (!usuario) {
        badgePostorEl.className = "badge-postor hidden";
        return;
    }

    const lideraId =
        sessionStorage.getItem(`lidera-${subastaId}`);

    const participo =
        sessionStorage.getItem(`participo-${subastaId}`) === "true";

    if (lideraId && parseInt(lideraId) === usuario.id) {

        badgePostorEl.className =
            "badge-postor badge-postor--lidera";

        badgePostorEl.innerHTML = `
            <i class="fa-solid fa-trophy"></i>

            <div class="badge-postor-textos">
                <strong>Estás liderando la subasta</strong>
                <span>Tu oferta actual es la más alta.</span>
            </div>
        `;

    }
    else if (participo) {

        badgePostorEl.className =
            "badge-postor badge-postor--outbid";

        badgePostorEl.innerHTML = `
            <i class="fa-solid fa-circle-exclamation"></i>

            <div class="badge-postor-textos">
                <strong>¡Fuiste superado!</strong>
                <span>Otro postor ofreció más que vos.</span>
            </div>
        `;

    }
    else {

        badgePostorEl.className =
            "badge-postor hidden";
    }
}

// ENVIAR PUJA RÁPIDA (botón "PUJAR $X")
async function enviarPujaRapida() {
    const monto = parseFloat(btnPujaRapEl?.dataset.monto ?? 0);
    await enviarPuja(monto);
}

// ENVIAR PUJA MANUAL (campo + botón "REALIZAR PUJA")
async function enviarPujaManual() {
    const monto = parseFloat(inputMontoEl?.value ?? 0);
    if (!monto || monto <= 0) {
        mostrarToast("Ingresá un monto válido.", "error");
        return;
    }
    await enviarPuja(monto);
}

// ENVIAR PUJA — lógica central con manejo de 201 / 400 / 409
async function enviarPuja(monto) {
    const usuario = obtenerUsuarioActivo();
    if (!usuario) { mostrarToast("Seleccioná un usuario activo.", "error"); return; }

    // Deshabilitar botones mientras procesa
    setBotonsPuja(true);

    try {
        await realizarPuja(subastaId, usuario.id, monto);

        // 201 — Éxito
        mostrarToast("¡Puja registrada con éxito!", "exito");
        sessionStorage.setItem(`lidera-${subastaId}`, usuario.id);
        sessionStorage.setItem(`participo-${subastaId}`, "true");
        if (inputMontoEl) inputMontoEl.value = "";

        // Refrescar inmediatamente sin esperar el polling
        await refrescar();

    } catch (err) {
        // err puede ser { tipo, mensaje } (lanzado por api.js) o un Error nativo
        const tipo    = err.tipo    ?? "ERROR";
        const mensaje = err.mensaje ?? err.message ?? "Error al registrar la puja.";

        if (tipo === "CONCURRENCIA") {
            mostrarToast(mensaje, "warn");

            sessionStorage.setItem(
                `participo-${subastaId}`,
                "true"
            );

            sessionStorage.removeItem(
                `lidera-${subastaId}`
            );
        }

        await refrescar();
    } finally {
        setBotonsPuja(false);
    }
}

function setBotonsPuja(deshabilitado) {
    if (btnPujaRapEl)  btnPujaRapEl.disabled  = deshabilitado;
    if (btnRealizarEl) btnRealizarEl.disabled  = deshabilitado;
}

// HISTORIAL DE PUJAS
function renderizarHistorial(pujas) {
    if (!historialEl) return;

    const cantidad = mostrandoTodas ? pujas.length : Math.min(pujas.length, 5);
    const mostrar  = pujas.slice(0, cantidad);

    historialEl.innerHTML = mostrar.length === 0
        ? `<tr><td colspan="4" class="historial-vacio">Aún no hay pujas en esta subasta.</td></tr>`
        : mostrar.map((p, i) => `
            <tr class="${i === 0 ? "historial-lider" : ""}">
                <td>${i + 1}</td>
                <td>
                    ${i === 0 ? '<i class="fa-solid fa-trophy historial-icon-lider"></i>' : ""}
                    ${esc(p.seudonimo ?? `Postor ${i + 1}`)}
                </td>
                <td>${moneda(p.monto)}</td>
                <td>${formatFecha(p.fechaHora)}</td>
            </tr>`).join("");

    if (btnVerTodasEl) {
        btnVerTodasEl.style.display = pujas.length <= 5 ? "none" : "flex";
        btnVerTodasEl.textContent   = mostrandoTodas
            ? "Ver menos pujas ↑"
            : `Ver todas las pujas (${pujas.length}) ↓`;
    }
}

function toggleVerTodas() {
    mostrandoTodas = !mostrandoTodas;
    if (estadoActual) obtenerPujas(subastaId).then(pujas => renderizarHistorial(pujas));
}

// TABS (Descripción / Características / Vendedor)
function cambiarTab(btnActivo) {
    document.querySelectorAll(".tab-btn").forEach(b => b.classList.remove("activo"));
    document.querySelectorAll(".tab-panel").forEach(p => p.classList.remove("activo"));
    btnActivo.classList.add("activo");
    const panel = document.getElementById(btnActivo.dataset.tab);
    if (panel) panel.classList.add("activo");
}

// TOAST — notificación flotante
function mostrarToast(mensaje, tipo = "info") {
    const colores = {
        exito: "#22c55e",
        error: "#ef4444",
        warn:  "#f59e0b",
        info:  "#3b82f6"
    };
    const iconos = {
        exito: "fa-circle-check",
        error: "fa-circle-xmark",
        warn:  "fa-triangle-exclamation",
        info:  "fa-circle-info"
    };

    const toast = document.createElement("div");
    toast.className = "sala-toast";
    toast.style.borderLeftColor = colores[tipo] ?? colores.info;
    toast.innerHTML = `
        <i class="fa-solid ${iconos[tipo] ?? iconos.info}" style="color:${colores[tipo] ?? colores.info}"></i>
        <span>${esc(mensaje)}</span>
    `;
    document.getElementById("toast-container")?.appendChild(toast);

    // Animar entrada
    requestAnimationFrame(() => toast.classList.add("sala-toast--visible"));
    // Remover a los 4 segundos
    setTimeout(() => {
        toast.classList.remove("sala-toast--visible");
        toast.addEventListener("transitionend", () => toast.remove());
    }, 4000);
}

// UTILITARIOS
function detenerPolling() {
    clearInterval(intervaloPoll);
    clearInterval(intervaloTimer);
}

function mostrarError(msg) {
    document.body.innerHTML = `
        <div style="display:flex;align-items:center;justify-content:center;height:100vh;flex-direction:column;gap:16px;font-family:sans-serif">
            <i class="fa-solid fa-triangle-exclamation" style="font-size:3rem;color:#f59e0b"></i>
            <p style="font-size:1.1rem;color:#374151">${msg}</p>
            <a href="index.html" style="color:#3b82f6;text-decoration:underline">Volver al catálogo</a>
        </div>`;
}

function msTiempo(ms) {
    const s  = Math.floor(ms / 1000);
    const hh = Math.floor(s / 3600);
    const mm = Math.floor((s % 3600) / 60);
    const ss = s % 60;
    return [hh, mm, ss].map(n => String(n).padStart(2, "0")).join(":");
}

function moneda(v) {
    return new Intl.NumberFormat("es-AR", {
        style: "currency", currency: "ARS",
        minimumFractionDigits: 0, maximumFractionDigits: 0
    }).format(v ?? 0);
}

function esc(txt) {
    return String(txt ?? "")
        .replace(/&/g,"&amp;").replace(/</g,"&lt;")
        .replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}

function etiquetaEstado(e) {
    return { ACTIVA:"ACTIVA", PROGRAMADA:"PRÓXIMA", FINALIZADA:"FINALIZADA", DESIERTA:"DESIERTA" }[e] ?? e;
}

function formatFecha(iso) {
    if (!iso) return "—";
    const d = new Date(iso);
    return d.toLocaleDateString("es-AR", { day:"2-digit", month:"2-digit" })
        + " " + d.toLocaleTimeString("es-AR", { hour:"2-digit", minute:"2-digit" });
}

// ── Arrancar
document.addEventListener("DOMContentLoaded", inicializar);
