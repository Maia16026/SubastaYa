// ====================================================
// USUARIOS DISPONIBLES
// =====================================================

// Usuarios de prueba que existen en el Seed.
// La contraseña se utiliza solamente para simular el Login
// desde el frontend.

const USUARIOS_SEED = [
    {
        id: 1,
        nombre: "Vendedor",
        email: "vendedor@test.com",
        password: "123456"
    },
    {
        id: 2,
        nombre: "Comprador 1",
        email: "comprador1@test.com",
        password: "123456"
    },
    {
        id: 3,
        nombre: "Comprador 2",
        email: "comprador2@test.com",
        password: "123456"
    },
    {
        id: 4,
        nombre: "Sin Fondos",
        email: "sinfondos@test.com",
        password: "123456"
    }
];


// =====================================================
// ELEMENTOS DEL LOGIN
// =====================================================

const formLogin = document.getElementById("formLogin");

const email = document.getElementById("email");
const password = document.getElementById("password");

const errorEmail = document.getElementById("errorEmail");
const errorPassword = document.getElementById("errorPassword");

const mensajeError = document.getElementById("mensajeError");
const textoError = document.getElementById("textoError");

const btnLogin = document.getElementById("btnLogin");
const textoBtnLogin = document.getElementById("textoBtnLogin");
const spinnerLogin = document.getElementById("spinnerLogin");

const btnMostrarPassword = document.getElementById("btnMostrarPassword");
const iconoPassword = document.getElementById("iconoPassword");

const btnRegistro = document.getElementById("btnRegistro");

const seccionCuentasVinculadas =
    document.getElementById("seccionCuentasVinculadas");

const listaCuentasVinculadas =
    document.getElementById("listaCuentasVinculadas");


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

    const cuentaParaGuardar = {
        id: usuario.id,
        nombre: usuario.nombre,
        email: usuario.email
    };

    cuentas.push(cuentaParaGuardar);

    localStorage.setItem(
        "cuentasVinculadas",
        JSON.stringify(cuentas)
    );
}


function renderizarCuentasVinculadas() {

    const cuentas = obtenerCuentasVinculadas();

    listaCuentasVinculadas.innerHTML = "";

    if (cuentas.length === 0) {
        seccionCuentasVinculadas.classList.add("d-none");
        return;
    }

    seccionCuentasVinculadas.classList.remove("d-none");

    cuentas.forEach(function (cuenta) {

        const boton = document.createElement("button");

        boton.type = "button";
        boton.className = "cuenta-vinculada";

        boton.innerHTML = `
            <span class="cuenta-vinculada-icono">
                ${cuenta.nombre.charAt(0).toUpperCase()}
            </span>

            <span class="cuenta-vinculada-datos">
                <span class="cuenta-vinculada-nombre">
                    ${cuenta.nombre}
                </span>

                <span class="cuenta-vinculada-email">
                    ${cuenta.email}
                </span>
            </span>

            <i class="bi bi-chevron-right cuenta-vinculada-flecha"></i>
        `;

        boton.addEventListener("click", function () {
            iniciarSesionConCuentaVinculada(cuenta.id);
        });

        listaCuentasVinculadas.appendChild(boton);
    });
}


// =====================================================
// INICIAR SESIÓN
// =====================================================

function guardarUsuarioActivo(usuario) {

    const usuarioActivo = {
        id: usuario.id,
        nombre: usuario.nombre,
        email: usuario.email
    };

    sessionStorage.setItem(
        "usuarioActivo",
        JSON.stringify(usuarioActivo)
    );
}


function iniciarSesion(usuario) {

    guardarUsuarioActivo(usuario);
    guardarCuentaVinculada(usuario);

    window.location.href = "index.html";
}


function iniciarSesionConCuentaVinculada(usuarioId) {

    const usuario = USUARIOS_SEED.find(function (usuarioSeed) {
        return usuarioSeed.id === usuarioId;
    });

    if (!usuario) {
        return;
    }

    iniciarSesion(usuario);
}


// =====================================================
// LIMPIAR ERRORES
// =====================================================

function limpiarErrores() {

    errorEmail.classList.add("d-none");
    errorPassword.classList.add("d-none");
    mensajeError.classList.add("d-none");

    email.closest(".input-login").classList.remove("input-error");
    password.closest(".input-login").classList.remove("input-error");
}


// =====================================================
// VALIDAR CAMPOS
// =====================================================

function validarCampos() {

    let formularioValido = true;

    const emailIngresado = email.value.trim();
    const passwordIngresada = password.value.trim();

    if (emailIngresado === "") {

        errorEmail.textContent = "Ingresá tu email.";
        errorEmail.classList.remove("d-none");

        email.closest(".input-login").classList.add("input-error");

        formularioValido = false;

    } else if (!email.checkValidity()) {

        errorEmail.textContent = "Ingresá un email válido.";
        errorEmail.classList.remove("d-none");

        email.closest(".input-login").classList.add("input-error");

        formularioValido = false;
    }

    if (passwordIngresada === "") {

        errorPassword.textContent = "Ingresá tu contraseña.";
        errorPassword.classList.remove("d-none");

        password.closest(".input-login").classList.add("input-error");

        formularioValido = false;
    }

    return formularioValido;
}


// =====================================================
// ESTADO CARGANDO
// =====================================================

function mostrarCargando() {

    btnLogin.disabled = true;

    spinnerLogin.classList.remove("d-none");

    textoBtnLogin.textContent = "INICIANDO SESIÓN...";
}


function ocultarCargando() {

    btnLogin.disabled = false;

    spinnerLogin.classList.add("d-none");

    textoBtnLogin.textContent = "INICIAR SESIÓN";
}


// =====================================================
// CREDENCIALES INCORRECTAS
// =====================================================

function mostrarCredencialesIncorrectas() {

    textoError.textContent =
        "El email o la contraseña son incorrectos.";

    mensajeError.classList.remove("d-none");
}


// =====================================================
// MOSTRAR / OCULTAR CONTRASEÑA
// =====================================================

btnMostrarPassword.addEventListener("click", function () {

    if (password.type === "password") {

        password.type = "text";

        iconoPassword.classList.remove("bi-eye");
        iconoPassword.classList.add("bi-eye-slash");

        btnMostrarPassword.setAttribute(
            "aria-label",
            "Ocultar contraseña"
        );

    } else {

        password.type = "password";

        iconoPassword.classList.remove("bi-eye-slash");
        iconoPassword.classList.add("bi-eye");

        btnMostrarPassword.setAttribute(
            "aria-label",
            "Mostrar contraseña"
        );
    }
});


// =====================================================
// QUITAR ERRORES AL VOLVER A ESCRIBIR
// =====================================================

email.addEventListener("input", function () {

    errorEmail.classList.add("d-none");

    email.closest(".input-login").classList.remove("input-error");

    mensajeError.classList.add("d-none");
});


password.addEventListener("input", function () {

    errorPassword.classList.add("d-none");

    password.closest(".input-login").classList.remove("input-error");

    mensajeError.classList.add("d-none");
});


// =====================================================
// ENVIAR LOGIN
// =====================================================

formLogin.addEventListener("submit", function (event) {

    event.preventDefault();

    limpiarErrores();

    const formularioValido = validarCampos();

    if (!formularioValido) {
        return;
    }

    mostrarCargando();

    const emailIngresado = email.value.trim().toLowerCase();
    const passwordIngresada = password.value.trim();

    const usuarioEncontrado = USUARIOS_SEED.find(function (usuario) {

        const mismoEmail =
            usuario.email.toLowerCase() === emailIngresado;

        const mismaPassword =
            usuario.password === passwordIngresada;

        return mismoEmail && mismaPassword;
    });

    setTimeout(function () {

        if (usuarioEncontrado) {

            iniciarSesion(usuarioEncontrado);
            return;
        }

        ocultarCargando();
        mostrarCredencialesIncorrectas();

    }, 600);
});


// =====================================================
// REGISTRO OPCIONAL
// =====================================================

btnRegistro.addEventListener("click", function () {

    textoError.textContent =
        "El registro de usuarios es una funcionalidad opcional.";

    mensajeError.classList.remove("d-none");
});


// =====================================================
// INICIALIZACIÓN
// =====================================================

document.addEventListener("DOMContentLoaded", function () {

    renderizarCuentasVinculadas();
});