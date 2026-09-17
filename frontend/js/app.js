// app.js - Inicializador principal del frontend
// Gestiona la sesión y el usuario activo usando los usuarios del Seed.

import { inicializarCatalogo } from "./catalogo.js";


// =====================================================
// USUARIOS DEL SEED
// =====================================================

export const USUARIOS_SEED = [
    { id: 1, nombre: "Vendedor", email: "vendedor@test.com" },
    { id: 2, nombre: "Comprador 1", email: "comprador1@test.com" },
    { id: 3, nombre: "Comprador 2", email: "comprador2@test.com" },
    { id: 4, nombre: "Sin Fondos", email: "sinfondos@test.com" }
];


// =====================================================
// USUARIO ACTIVO
// =====================================================

export function obtenerUsuarioActivo() {

    const guardado = sessionStorage.getItem("usuarioActivo");

    if (!guardado) {
        return null;
    }

    return JSON.parse(guardado);
}


function guardarUsuarioActivo(usuario) {

    sessionStorage.setItem(
        "usuarioActivo",
        JSON.stringify(usuario)
    );
}


export function haySesionIniciada() {

    return obtenerUsuarioActivo() !== null;
}


// =====================================================
// CUENTAS VINCULADAS
// =====================================================

function obtenerCuentasVinculadas() {

    const guardadas = localStorage.getItem("cuentasVinculadas");

    if (!guardadas) {
        return [];
    }

    return JSON.parse(guardadas);
}


function guardarCuentaVinculada(usuario) {

    const cuentas = obtenerCuentasVinculadas();

    const yaExiste = cuentas.some(function (cuenta) {
        return cuenta.id === usuario.id;
    });

    if (yaExiste) {
        return;
    }

    cuentas.push({
        id: usuario.id,
        nombre: usuario.nombre,
        email: usuario.email
    });

    localStorage.setItem(
        "cuentasVinculadas",
        JSON.stringify(cuentas)
    );
}


// =====================================================
// SELECTOR VIEJO
// =====================================================

// Mientras terminamos de reemplazar visualmente el selector
// de todas las páginas, mantenemos esta lógica para que
// las vistas actuales sigan funcionando.

function inicializarSelectorUsuario() {

    const selector = document.getElementById("selector-usuario");

    if (!selector) {
        return;
    }

    const usuarioActivo = obtenerUsuarioActivo();

    // Si todavía no hay sesión, el selector no debe mostrarse.
    if (!usuarioActivo) {
        selector.classList.add("d-none");
        return;
    }

    selector.innerHTML = "";

    USUARIOS_SEED.forEach(function (usuario) {

        const opcion = document.createElement("option");

        opcion.value = usuario.id;
        opcion.textContent =
            `${usuario.nombre} (${usuario.email})`;

        selector.appendChild(opcion);
    });

    selector.value = usuarioActivo.id;

    actualizarIconoUsuario(usuarioActivo);

    selector.addEventListener("change", function () {

        const usuarioSeleccionado = USUARIOS_SEED.find(
            function (usuario) {
                return usuario.id === Number(selector.value);
            }
        );

        if (!usuarioSeleccionado) {
            return;
        }

        guardarUsuarioActivo(usuarioSeleccionado);
        guardarCuentaVinculada(usuarioSeleccionado);

        actualizarIconoUsuario(usuarioSeleccionado);

        window.dispatchEvent(
            new CustomEvent(
                "usuarioCambiado",
                { detail: usuarioSeleccionado }
            )
        );
    });
}


function actualizarIconoUsuario(usuario) {

    if (!usuario) {
        return;
    }

    const inicial =
        usuario.nombre.charAt(0).toUpperCase();

    const iconoNavbar =
        document.getElementById("usuario-icono-letra");

    const iconoMenu =
        document.getElementById("menuUsuarioIcono");

    if (iconoNavbar) {
        iconoNavbar.textContent = inicial;
    }

    if (iconoMenu) {
        iconoMenu.textContent = inicial;
    }
}

// =====================================================
// NAVEGACIÓN SEGÚN SESIÓN
// =====================================================

function actualizarNavegacionSegunSesion() {

    const usuarioActivo = obtenerUsuarioActivo();

    const opcionesSoloSesion =
        document.querySelectorAll(".nav-solo-sesion");

    const navIniciarSesion =
        document.getElementById("navIniciarSesion");

    const navCuenta =
        document.getElementById("navCuenta");

    // =================================================
    // SIN SESIÓN
    // =================================================

    if (!usuarioActivo) {

        opcionesSoloSesion.forEach(function (opcion) {
            opcion.classList.add("d-none");
        });

        if (navIniciarSesion) {
            navIniciarSesion.classList.remove("d-none");
        }

        if (navCuenta) {
            navCuenta.classList.add("d-none");
        }

        return;
    }


    // =================================================
    // CON SESIÓN
    // =================================================

    opcionesSoloSesion.forEach(function (opcion) {
        opcion.classList.remove("d-none");
    });

    if (navIniciarSesion) {
        navIniciarSesion.classList.add("d-none");
    }

    if (navCuenta) {
        navCuenta.classList.remove("d-none");
    }

    completarMenuCuenta(usuarioActivo);
}

// =====================================================
// MENÚ DE CUENTA
// =====================================================

function completarMenuCuenta(usuarioActivo) {

    const nombreNavbar =
        document.getElementById("navNombreUsuario");

    const nombreMenu =
        document.getElementById("menuNombreUsuario");

    const emailMenu =
        document.getElementById("menuEmailUsuario");

    if (nombreNavbar) {
        nombreNavbar.textContent = usuarioActivo.nombre;
    }

    if (nombreMenu) {
        nombreMenu.textContent = usuarioActivo.nombre;
    }

    if (emailMenu) {
        emailMenu.textContent = usuarioActivo.email;
    }

    actualizarIconoUsuario(usuarioActivo);

    renderizarCuentasDelMenu(usuarioActivo);
    inicializarAccionesCuenta();
}


function renderizarCuentasDelMenu(usuarioActivo) {

    const contenedor =
        document.getElementById("menuCuentasVinculadas");

    if (!contenedor) {
        return;
    }

    contenedor.innerHTML = "";

    const cuentasVinculadas = obtenerCuentasVinculadas();

    const otrasCuentas = cuentasVinculadas.filter(
        function (cuenta) {
            return cuenta.id !== usuarioActivo.id;
        }
    );

    otrasCuentas.forEach(function (cuenta) {

        const boton = document.createElement("button");

        boton.type = "button";
        boton.className = "dropdown-item menu-cuenta-vinculada";

        boton.innerHTML = `
            <span class="menu-cuenta-mini-icono">
                ${cuenta.nombre.charAt(0).toUpperCase()}
            </span>

            <span>
                <strong>${cuenta.nombre}</strong>
                <small>${cuenta.email}</small>
            </span>
        `;

        boton.addEventListener("click", function () {
            cambiarCuenta(cuenta.id);
        });

        contenedor.appendChild(boton);
    });
}


function cambiarCuenta(usuarioId) {

    const usuarioSeleccionado = USUARIOS_SEED.find(
        function (usuario) {
            return usuario.id === usuarioId;
        }
    );

    if (!usuarioSeleccionado) {
        return;
    }

    guardarUsuarioActivo(usuarioSeleccionado);

    window.dispatchEvent(
        new CustomEvent(
            "usuarioCambiado",
            { detail: usuarioSeleccionado }
        )
    );

    window.location.reload();
}


function inicializarAccionesCuenta() {

    const btnOtraCuenta =
        document.getElementById("btnOtraCuenta");

    const btnCerrarSesion =
        document.getElementById("btnCerrarSesion");

    if (btnOtraCuenta) {

        btnOtraCuenta.onclick = function () {
            window.location.href = "login.html";
        };
    }

    if (btnCerrarSesion) {

        btnCerrarSesion.onclick = function () {

            sessionStorage.removeItem("usuarioActivo");

            window.location.href = "index.html";
        };
    }
}

// =====================================================
// PROTEGER PÁGINAS PRIVADAS
// =====================================================

function protegerPaginaPrivada() {

    const pagina =
        window.location.pathname.split("/").pop();

    const paginasPrivadas = [
        "publicar.html",
        "billetera.html",
        "actividades.html"
    ];

    const esPaginaPrivada =
        paginasPrivadas.includes(pagina);

    if (esPaginaPrivada && !haySesionIniciada()) {

        window.location.href = "login.html";

        return false;
    }

    return true;
}


// =====================================================
// MARCAR LINK ACTIVO
// =====================================================

function marcarNavActivo() {

    const pagina =
        window.location.pathname.split("/").pop()
        || "index.html";

    document.querySelectorAll(".navbar__links a")
        .forEach(function (link) {

            link.classList.toggle(
                "activo",
                link.dataset.pagina === pagina
            );
        });
}


// =====================================================
// PUNTO DE ENTRADA
// =====================================================

document.addEventListener("DOMContentLoaded", function () {

    const paginaPermitida = protegerPaginaPrivada();

    if (!paginaPermitida) {
        return;
    }

    actualizarNavegacionSegunSesion();

    inicializarSelectorUsuario();

    marcarNavActivo();

    if (document.getElementById("grid-subastas")) {
        inicializarCatalogo();
    }
});