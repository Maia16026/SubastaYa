// app.js — Inicializador principal del frontend
// Gestiona el usuario activo (simulado con los datos seed)

import { inicializarCatalogo } from "./catalogo.js";

// Usuarios de prueba del Seed (tal cual están en la base de datos)
const USUARIOS_SEED = [
    { id: 1, nombre: "Vendedor",   email: "vendedor@test.com" },
    { id: 2, nombre: "Comprador 1", email: "comprador1@test.com" },
    { id: 3, nombre: "Comprador 2", email: "comprador2@test.com" },
    { id: 4, nombre: "Sin Fondos",  email: "sinfondos@test.com"  },
];

// El usuario activo se guarda en sessionStorage para persistir entre páginas
export function obtenerUsuarioActivo() {
    const guardado = sessionStorage.getItem("usuarioActivo");
    return guardado ? JSON.parse(guardado) : USUARIOS_SEED[1]; // comprador1 por defecto
}

function guardarUsuarioActivo(usuario) {
    sessionStorage.setItem("usuarioActivo", JSON.stringify(usuario));
}

// ---- Renderizar selector de usuario en el navbar ----
function inicializarSelectorUsuario() {
    const selector = document.getElementById("selector-usuario");
    if (!selector) return;

    // Poblar opciones
    USUARIOS_SEED.forEach(u => {
        const opt = document.createElement("option");
        opt.value = u.id;
        opt.textContent = `${u.nombre} (${u.email})`;
        selector.appendChild(opt);
    });

    // Preseleccionar el usuario activo
    const actual = obtenerUsuarioActivo();
    selector.value = actual.id;

    // Actualizar ícono de usuario
    actualizarIconoUsuario(actual);

    // Cambio de usuario
    selector.addEventListener("change", () => {
        const seleccionado = USUARIOS_SEED.find(u => u.id === Number(selector.value));
        if (seleccionado) {
            guardarUsuarioActivo(seleccionado);
            actualizarIconoUsuario(seleccionado);
            // Disparar evento para que otros módulos reaccionen (billetera, etc.)
            window.dispatchEvent(new CustomEvent("usuarioCambiado", { detail: seleccionado }));
        }
    });
}

function actualizarIconoUsuario(usuario) {
    const icono = document.getElementById("usuario-icono-letra");
    if (icono) icono.textContent = usuario.nombre.charAt(0).toUpperCase();
}

// ---- Marcar el link activo en el navbar ----
function marcarNavActivo() {
    const pagina = window.location.pathname.split("/").pop() || "index.html";
    document.querySelectorAll(".navbar__links a").forEach(link => {
        link.classList.toggle("activo", link.dataset.pagina === pagina);
    });
}

// ---- Punto de entrada ----
document.addEventListener("DOMContentLoaded", () => {
    inicializarSelectorUsuario();
    marcarNavActivo();

    // Si estamos en el catálogo (index.html), inicializarlo
    if (document.getElementById("grid-subastas")) {
        inicializarCatalogo();
    }
});
