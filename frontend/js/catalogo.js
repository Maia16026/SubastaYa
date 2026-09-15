// catalogo.js — Lógica de la Vista 1: Inicio / Catálogo

import { obtenerSubastas } from "./api.js";

// Mapa de categorías local (coincide con el Seed del backend)
// CategoriaId → nombre visible
const CATEGORIAS = {
    1: "Tecnología",
    2: "Coleccionables",
    3: "Indumentaria",
    4: "Vehículos"
};

// ---- Estado interno de la vista ----
let todasLasSubastas = [];  // caché de la última respuesta
let intervaloTimers  = null;

// ---- Nodos del DOM ----
const gridEl      = document.getElementById("grid-subastas");
const spinnerEl   = document.getElementById("spinner-global"); // Corregido ID
const vacioEl     = document.getElementById("estado-vacio");
const totalEl     = document.getElementById("total-resultados");
const buscadorEl  = document.getElementById("input-busqueda"); // Corregido ID
const formBusqueda = document.getElementById("form-busqueda"); // Agregado formulario
const selEstado   = document.getElementById("filtro-estado");
const selCat      = document.getElementById("filtro-categoria");
const inPrecioMin = document.getElementById("filtro-precio-min");
const inPrecioMax = document.getElementById("filtro-precio-max");
const selOrden    = document.getElementById("filtro-orden");
const btnLimpiar  = document.getElementById("btn-limpiar"); // Agregado botón limpiar

// Botones de categoría visuales
const categoryButtons = document.querySelectorAll(".category");

// INICIO — llamado desde app.js
export async function inicializarCatalogo() {
    await cargarSubastas();

    // Listeners de filtros que disparan recarga desde la API
    [selEstado, selCat, selOrden].forEach(el =>
        el?.addEventListener("change", cargarSubastas));
    [inPrecioMin, inPrecioMax].forEach(el =>
        el?.addEventListener("input", () => {
            clearTimeout(el._debounce);
            el._debounce = setTimeout(cargarSubastas, 300);
        }));

    // Buscador: filtra en el cliente
    buscadorEl?.addEventListener("input", filtrarEnCliente);
    formBusqueda?.addEventListener("submit", (e) => { e.preventDefault(); filtrarEnCliente(); });

    // Botón Limpiar
    btnLimpiar?.addEventListener("click", window.limpiarFiltros);

    // Botones de Categorías visuales
    categoryButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            // Actualizar diseño
            categoryButtons.forEach(b => b.classList.remove("active", "activo"));
            btn.classList.add("activo");
            
            // Sincronizar con el select nativo y recargar
            if (selCat) {
                selCat.value = btn.dataset.id === "0" ? "" : btn.dataset.id;
                cargarSubastas();
            }
        });
    });

    // Toggle de vista (por si se vuelve a usar en otra iteración, ya no están en HTML de compañera pero no rompe)
    inicializarToggleVista();
}

// TOGGLE DE VISTA: Tarjetas ↔ Lista
function inicializarToggleVista() {
    const botonesToggle = document.querySelectorAll(".vista-toggle button");
    if (botonesToggle.length < 2) return;

    const [btnTarjetas, btnLista] = botonesToggle;

    btnTarjetas.addEventListener("click", () => {
        // Activar vista de tarjetas (Grid)
        gridEl.classList.remove("vista-lista");
        btnTarjetas.classList.add("activo");
        btnTarjetas.setAttribute("aria-pressed", "true");
        btnLista.classList.remove("activo");
        btnLista.setAttribute("aria-pressed", "false");
    });

    btnLista.addEventListener("click", () => {
        // Activar vista de lista (Flex column)
        gridEl.classList.add("vista-lista");
        btnLista.classList.add("activo");
        btnLista.setAttribute("aria-pressed", "true");
        btnTarjetas.classList.remove("activo");
        btnTarjetas.setAttribute("aria-pressed", "false");
    });
}

// CARGAR SUBASTAS desde la 
async function cargarSubastas() {
    mostrarSpinner(true);
    detenerTimers();

    try {
        const filtros = {};

        // Estado: el select tiene "" para "Todos" o "ACTIVA", "PROGRAMADA", etc.
        if (selEstado?.value)   filtros.estado      = selEstado.value;
        if (selCat?.value)      filtros.categoriaId = selCat.value;
        if (inPrecioMin?.value) filtros.precioMin   = inPrecioMin.value;
        if (inPrecioMax?.value) filtros.precioMax   = inPrecioMax.value;
        if (selOrden?.value)    filtros.orden       = selOrden.value;

        const { items, totalRegistros } = await obtenerSubastas(filtros);
        todasLasSubastas = items;

        // Aplicar filtro de texto del buscador sobre lo recibido
        filtrarEnCliente();

    } catch (error) {
        console.error("Error al cargar subastas:", error);
        todasLasSubastas = [];
        renderizar([]);
    } finally {
        mostrarSpinner(false);
    }
}

// FILTRAR EN CLIENTE (solo por texto del buscador)
function filtrarEnCliente() {
    const texto = buscadorEl?.value.toLowerCase().trim() ?? "";

    const filtradas = texto
        ? todasLasSubastas.filter(s =>
            s.titulo?.toLowerCase().includes(texto) ||
            s.descripcion?.toLowerCase().includes(texto))
        : todasLasSubastas;

    renderizar(filtradas);
}

// RENDERIZAR el grid de cards
function renderizar(subastas) {
    detenerTimers();
    gridEl.innerHTML = "";

    /// Actualizar contador de resultados
    if (totalEl) {
        totalEl.textContent =
            `${subastas.length} subasta${subastas.length !== 1 ? "s" : ""}`;
    }

    if (subastas.length === 0) {
        vacioEl?.classList.add("visible");
        gridEl.style.display = "none";
        return;
    }

    vacioEl?.classList.remove("visible");
    gridEl.style.display = "";

    subastas.forEach(s => gridEl.appendChild(crearCard(s)));

    iniciarTimers();
}
// RESOLVEDOR DE IMÁGENES
function obtenerImagenProducto(subasta) {
    // Si el backend manda una URL externa real, la usamos.
    if (subasta.urlImagen?.startsWith("http")) {
        return subasta.urlImagen;
    }

    // Adaptamos las imágenes del Seed del backend
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

    // Si en algún momento el backend manda directamente
    // una imagen que existe en assets/img
    if (subasta.urlImagen) {
        const nombreArchivo = subasta.urlImagen.split("/").pop();
        return `assets/img/${nombreArchivo}`;
    }

    // Último recurso
    return "assets/img/notebook.jpg";
}

// CREAR UNA CARD de subasta (Bootstrap)
function crearCard(s) {
    const catNombre = CATEGORIAS[s.categoriaId] ?? "Categoría";
    
    // Contenedor Bootstrap: 3 columnas en desktop, 2 tablet, 1 móvil
    const col = document.createElement("div");
    col.className = "col-md-6 col-lg-4";

    // Status visual
    let badgeClass = "active-status";
    let estadoVisible = "ACTIVA";
    
    if (s.estado === "PROGRAMADA") {
        badgeClass = "upcoming-status";
        estadoVisible = "PRÓXIMA";
    }
    else if (s.estado === "FINALIZADA") {
        badgeClass = "finished-status";
        estadoVisible = "FINALIZADA";
    }
    else if (s.estado === "DESIERTA") {
        badgeClass = "finished-status";
        estadoVisible = "DESIERTA";
    }

    // Precio o puja
    let labelPrecio = "Precio base";

    if (s.pujaActual !== null && s.pujaActual !== undefined) {
        labelPrecio = "Puja actual";
    }

    if (s.estado === "FINALIZADA") {
        labelPrecio = "Precio final";
    }

    const montoMostrado = s.pujaActual ?? s.precioBase;

    let textoTiempo = "";
    let colorTexto = "var(--muted)";

    if (s.estado === "FINALIZADA" || s.estado === "DESIERTA") {

        textoTiempo = "Finalizada";

    }
    else if (s.estado === "PROGRAMADA") {

        textoTiempo = `
            <span class="countdown"
                data-fecha="${s.fechaInicio}"
                data-tipo="inicio">
                <span class="cd-texto">Calculando...</span>
            </span>
        `;

        colorTexto = "var(--blue)";

    }
    else {

        textoTiempo = `
            <span class="countdown"
                data-fecha="${s.fechaFin}"
                data-tipo="fin">
                <span class="cd-texto">Calculando...</span>
            </span>
        `;

        colorTexto = "var(--green)";
    }

    // Botón principal
    let textoBtn = "VER SUBASTA";
    let btnClass = "auction-button";
    if (s.estado === "FINALIZADA" || s.estado === "DESIERTA") {
        textoBtn = "VER RESULTADO";
        btnClass += " finished-button";
    }

    // Imagen
    const imgSrc = obtenerImagenProducto(s);

    col.innerHTML = `
        <article class="auction-card">
            <div class="auction-image">
                <img src="${imgSrc}" alt="${esc(s.titulo)}" class="product-image" loading="lazy">
                <!-- Favorito -->
                <button class="favorite-button" type="button" aria-label="Favorito">
                    <i class="bi bi-heart"></i>
                </button>
                <span class="auction-status ${badgeClass}">
                ${estadoVisible}
                </span>
            </div>
            
            <div class="auction-content">
                <h3>${esc(s.titulo)}</h3>
                <p class="auction-category">${esc(catNombre)}</p>
                
                <small>${labelPrecio}</small>
                <p class="auction-price">${moneda(montoMostrado)}</p>
                
                <div class="auction-info">
                    <span>
                        <i class="bi bi-hammer"></i>
                        ${s.cantidadPujas} oferta${s.cantidadPujas !== 1 ? "s" : ""}
                    </span>
                    <span style="color: ${colorTexto};">
                        <i class="bi bi-clock"></i>
                        ${textoTiempo}
                    </span>
                </div>
                
                <button class="${btnClass}" onclick="irASubasta(${s.id})">
                    ${textoBtn}
                </button>
            </div>
        </article>
    `;

    // Toggle de favorito
    col.querySelector(".favorite-button").addEventListener("click", e => {
        e.stopPropagation();
        const btn = e.currentTarget;
        const icon = btn.querySelector("i");
        if (icon.classList.contains("bi-heart")) {
            icon.className = "bi bi-heart-fill";
            icon.style.color = "var(--orange)";
        } else {
            icon.className = "bi bi-heart";
            icon.style.color = "";
        }
    });

    return col;
}

// TIMERS — countdown regresivo
function iniciarTimers() {
    actualizarTodosLosCountdowns();
    intervaloTimers = setInterval(actualizarTodosLosCountdowns, 1000);
}

function detenerTimers() {
    clearInterval(intervaloTimers);
    intervaloTimers = null;
}

function actualizarTodosLosCountdowns() {

    const ahora = Date.now();

    document.querySelectorAll(".countdown[data-fecha]").forEach(el => {

        const fecha = new Date(el.dataset.fecha).getTime();
        const restanMs = fecha - ahora;

        const textoEl = el.querySelector(".cd-texto") ?? el;

        if (restanMs <= 0) {

            if (el.dataset.tipo === "inicio") {
                textoEl.textContent = "Comenzando...";
                el.className = "countdown countdown--inactivo";
            } else {
                textoEl.textContent = "Finalizada";
                el.className = "countdown countdown--inactivo";
            }

            return;
        }

        textoEl.textContent = msTiempo(restanMs);

        el.className = "countdown";

        if (restanMs < 60_000) {
            el.classList.add("countdown--critico");
        }
        else if (restanMs < 120_000) {
            el.classList.add("countdown--warn");
        }
    });
}

// UTILIDADES

function msTiempo(ms) {
    const s  = Math.floor(ms / 1000);
    const hh = Math.floor(s / 3600);
    const mm = Math.floor((s % 3600) / 60);
    const ss = s % 60;
    return [hh, mm, ss].map(n => String(n).padStart(2, "0")).join(":");
}

function moneda(valor) {
    return new Intl.NumberFormat("es-AR", {
        style: "currency", currency: "ARS",
        minimumFractionDigits: 0, maximumFractionDigits: 0
    }).format(valor);
}

function esc(txt) {
    return String(txt ?? "")
        .replace(/&/g,"&amp;").replace(/</g,"&lt;")
        .replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}

function etiquetaEstado(e) {
    return { ACTIVA:"ACTIVA", PROGRAMADA:"PRÓXIMA", FINALIZADA:"FINALIZADA", DESIERTA:"DESIERTA" }[e] ?? e;
}

function textoEstadoTimer(e) {
    if (e === "PROGRAMADA") return "⏳ Próxima";
    return "✔ Finalizada";
}

function mostrarSpinner(vis) {
    spinnerEl?.classList.toggle("visible", vis);
    if (vis) { gridEl.style.display = "none"; vacioEl?.classList.remove("visible"); }
}

// ---- Funciones globales (usadas desde el HTML) ----
window.irASubasta = id => { window.location.href = `sala.html?id=${id}`; };
window.limpiarFiltros = () => {
    [selEstado, selCat, selOrden].forEach(el => el && (el.value = ""));
    [inPrecioMin, inPrecioMax].forEach(el => el && (el.value = ""));
    if (buscadorEl) buscadorEl.value = "";
    
    // Resetear diseño de botones de categorías
    categoryButtons.forEach(b => {
        b.classList.remove("active", "activo");
        if (b.dataset.id === "0") b.classList.add("activo");
    });
    
    cargarSubastas();
};
