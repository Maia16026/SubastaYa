// =========================================================
// MI BILLETERA
// Conexión de la vista billetera.html con la API real.
// =========================================================

import {
    obtenerBilletera,
    depositarSaldo,
    obtenerTransaccionesBilletera
} from "./api.js";

import { obtenerUsuarioActivo } from "./app.js";


// =========================================================
// ELEMENTOS DE LA PÁGINA
// =========================================================

const saldoTotalElemento = document.getElementById("saldoTotal");
const saldoRetenidoElemento = document.getElementById("saldoRetenido");
const saldoDisponibleElemento = document.getElementById("saldoDisponible");

const formAcreditar = document.getElementById("formAcreditar");
const montoInput = document.getElementById("monto");
const errorMonto = document.getElementById("errorMonto");

const btnAcreditar = document.getElementById("btnAcreditar");
const iconoAcreditar = document.getElementById("iconoAcreditar");
const textoAcreditar = document.getElementById("textoAcreditar");
const spinnerAcreditar = document.getElementById("spinnerAcreditar");

const mensajeExito = document.getElementById("mensajeExito");
const tituloExito = document.getElementById("tituloExito");
const textoExito = document.getElementById("textoExito");

const listaMovimientos = document.getElementById("listaMovimientos");
const sinMovimientos = document.getElementById("sinMovimientos");
const cuerpoMovimientos = document.getElementById("cuerpoMovimientos");


// =========================================================
// ESTADO DE LA VISTA
// =========================================================

let procesando = false;
let montoTocado = false;


// =========================================================
// FORMATO DE DINERO
// =========================================================

function formatearDinero(monto) {
    const numero = Number(monto);

    return "$" + numero.toLocaleString("es-AR", {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    });
}


// =========================================================
// FORMATO DE FECHA
// =========================================================

function formatearFecha(fechaBackend) {
    if (!fechaBackend) {
        return "-";
    }

    const fecha = new Date(fechaBackend);

    if (Number.isNaN(fecha.getTime())) {
        return fechaBackend;
    }

    const dia = String(fecha.getDate()).padStart(2, "0");
    const mes = String(fecha.getMonth() + 1).padStart(2, "0");
    const anio = fecha.getFullYear();

    const hora = String(fecha.getHours()).padStart(2, "0");
    const minutos = String(fecha.getMinutes()).padStart(2, "0");

    return `${dia}/${mes}/${anio} - ${hora}:${minutos}`;
}


// =========================================================
// MOSTRAR SALDOS
// =========================================================

function mostrarSaldos(billetera) {
    saldoTotalElemento.textContent =
        formatearDinero(billetera.saldoTotal);

    saldoRetenidoElemento.textContent =
        formatearDinero(billetera.saldoRetenido);

    saldoDisponibleElemento.textContent =
        formatearDinero(billetera.saldoDisponible);
}


// =========================================================
// VALIDACIÓN DEL MONTO
// =========================================================

function obtenerErrorMonto() {
    const valor = montoInput.value.trim();

    if (valor === "") {
        return "Ingresá un monto.";
    }

    const monto = Number(valor);

    if (!Number.isFinite(monto) || monto <= 0) {
        return "El monto debe ser mayor a 0.";
    }

    return "";
}


function actualizarErrorMonto() {
    const error = obtenerErrorMonto();

    if (!montoTocado) {
        montoInput.classList.remove("is-invalid");
        errorMonto.textContent = "";
        return;
    }

    if (error !== "") {
        montoInput.classList.add("is-invalid");
        errorMonto.textContent = error;
    } else {
        montoInput.classList.remove("is-invalid");
        errorMonto.textContent = "";
    }
}


// =========================================================
// BOTÓN ACREDITAR
// =========================================================

function actualizarBotonAcreditar() {
    const error = obtenerErrorMonto();

    if (error === "" && !procesando) {
        btnAcreditar.disabled = false;
    } else {
        btnAcreditar.disabled = true;
    }
}


// =========================================================
// MENSAJES
// =========================================================

function ocultarMensajeExito() {
    mensajeExito.classList.add("d-none");
}


function mostrarMensajeExito(monto) {
    tituloExito.textContent =
        "¡Saldo acreditado correctamente!";

    textoExito.textContent =
        "Se acreditaron " +
        formatearDinero(monto) +
        " a tu billetera.";

    mensajeExito.classList.remove("d-none");
}


function mostrarErrorGeneral(mensaje) {
    tituloExito.textContent = "No se pudo completar la operación.";
    textoExito.textContent = mensaje;

    mensajeExito.classList.remove("d-none");
}


// =========================================================
// MOVIMIENTOS
// =========================================================

function obtenerDatosVisualesMovimiento(movimiento) {
    const tipo = String(movimiento.tipo ?? "").toUpperCase();

    let claseTipo = "";
    let claseMonto = "";
    let signo = "";
    let detalle = "";

    if (tipo === "DEPOSITO") {
        claseTipo = "tipo-deposito";
        claseMonto = "movimiento-positivo";
        signo = "+ ";
        detalle = "Acreditación manual";
    } else if (tipo === "RETENCION") {
        claseTipo = "tipo-retencion";
        claseMonto = "movimiento-negativo";
        signo = "- ";
        detalle = "Puja en subasta";
    } else if (tipo === "LIBERACION") {
        claseTipo = "tipo-liberacion";
        claseMonto = "movimiento-positivo";
        signo = "+ ";
        detalle = "Liberación de saldo retenido";
    } else if (tipo === "PAGO") {
        claseTipo = "tipo-pago";
        claseMonto = "movimiento-negativo";
        signo = "- ";
        detalle = "Compra finalizada";
    } else if (tipo === "COBRO") {
        claseTipo = "tipo-cobro";
        claseMonto = "movimiento-positivo";
        signo = "+ ";
        detalle = "Venta de subasta";
    } else {
        claseTipo = "";
        claseMonto = "";
        signo = "";
        detalle = "Movimiento de billetera";
    }

    return {
        tipo: tipo,
        claseTipo: claseTipo,
        claseMonto: claseMonto,
        signo: signo,
        detalle: detalle
    };
}


function mostrarMovimientos(movimientos) {
    cuerpoMovimientos.innerHTML = "";

    if (!Array.isArray(movimientos) || movimientos.length === 0) {
        listaMovimientos.classList.add("d-none");
        sinMovimientos.classList.remove("d-none");
        return;
    }

    movimientos.forEach(function (movimiento) {
        const datosVisuales =
            obtenerDatosVisualesMovimiento(movimiento);

        const subasta =
            movimiento.subastaId === null ||
            movimiento.subastaId === undefined
                ? "-"
                : "#" + movimiento.subastaId;

        const fila = document.createElement("tr");

        const celdaFecha = document.createElement("td");
        celdaFecha.textContent =
            formatearFecha(movimiento.fecha);

        const celdaTipo = document.createElement("td");
        const etiquetaTipo = document.createElement("span");

        etiquetaTipo.className =
            "tipo-movimiento " + datosVisuales.claseTipo;

        etiquetaTipo.textContent = datosVisuales.tipo;

        celdaTipo.appendChild(etiquetaTipo);

        const celdaMonto = document.createElement("td");
        celdaMonto.className = datosVisuales.claseMonto;
        celdaMonto.textContent =
            datosVisuales.signo +
            formatearDinero(movimiento.monto);

        const celdaSubasta = document.createElement("td");
        celdaSubasta.textContent = subasta;

        const celdaDetalle = document.createElement("td");
        celdaDetalle.textContent = datosVisuales.detalle;

        fila.appendChild(celdaFecha);
        fila.appendChild(celdaTipo);
        fila.appendChild(celdaMonto);
        fila.appendChild(celdaSubasta);
        fila.appendChild(celdaDetalle);

        cuerpoMovimientos.appendChild(fila);
    });

    listaMovimientos.classList.remove("d-none");
    sinMovimientos.classList.add("d-none");
}


// =========================================================
// CARGAR BILLETERA DESDE EL BACKEND
// =========================================================

async function cargarBilletera() {
    const usuario = obtenerUsuarioActivo();

    try {
        ocultarMensajeExito();

        const billetera = await obtenerBilletera(usuario.id);

        const movimientos =
            await obtenerTransaccionesBilletera(usuario.id);

        mostrarSaldos(billetera);
        mostrarMovimientos(movimientos);

    } catch (error) {
        console.error("Error al cargar la billetera:", error);

        mostrarErrorGeneral(
            error.message ??
            "No se pudo cargar la billetera."
        );
    }
}


// =========================================================
// ESTADO PROCESANDO
// =========================================================

function mostrarProcesando() {
    procesando = true;

    btnAcreditar.disabled = true;

    iconoAcreditar.classList.add("d-none");
    spinnerAcreditar.classList.remove("d-none");

    textoAcreditar.textContent = "PROCESANDO...";
}


function terminarProcesando() {
    procesando = false;

    iconoAcreditar.classList.remove("d-none");
    spinnerAcreditar.classList.add("d-none");

    textoAcreditar.textContent = "ACREDITAR SALDO";

    actualizarBotonAcreditar();
}


// =========================================================
// EVENTOS DEL CAMPO MONTO
// =========================================================

montoInput.addEventListener("input", function () {
    ocultarMensajeExito();

    if (montoTocado) {
        actualizarErrorMonto();
    }

    actualizarBotonAcreditar();
});


montoInput.addEventListener("blur", function () {
    montoTocado = true;

    actualizarErrorMonto();
    actualizarBotonAcreditar();
});


montoInput.addEventListener("change", function () {
    montoTocado = true;

    actualizarErrorMonto();
    actualizarBotonAcreditar();
});


// =========================================================
// ACREDITAR SALDO REAL
// =========================================================

formAcreditar.addEventListener("submit", async function (evento) {
    evento.preventDefault();

    if (procesando) {
        return;
    }

    montoTocado = true;
    actualizarErrorMonto();

    const error = obtenerErrorMonto();

    if (error !== "") {
        actualizarBotonAcreditar();
        return;
    }

    const monto = Number(montoInput.value);
    const usuario = obtenerUsuarioActivo();

    mostrarProcesando();
    ocultarMensajeExito();

    try {
        const billeteraActualizada =
            await depositarSaldo(usuario.id, monto);

        mostrarSaldos(billeteraActualizada);

        const movimientosActualizados =
            await obtenerTransaccionesBilletera(usuario.id);

        mostrarMovimientos(movimientosActualizados);

        mostrarMensajeExito(monto);

        montoInput.value = "";
        montoTocado = false;

        montoInput.classList.remove("is-invalid");
        errorMonto.textContent = "";

    } catch (error) {
        console.error("Error al acreditar saldo:", error);

        mostrarErrorGeneral(
            error.message ??
            "No se pudo acreditar el saldo."
        );

    } finally {
        terminarProcesando();
    }
});


// =========================================================
// CAMBIO DE USUARIO
// =========================================================

window.addEventListener("usuarioCambiado", async function () {
    montoInput.value = "";
    montoTocado = false;

    montoInput.classList.remove("is-invalid");
    errorMonto.textContent = "";

    actualizarBotonAcreditar();

    await cargarBilletera();
});


// =========================================================
// ESTADO INICIAL
// =========================================================

actualizarBotonAcreditar();
cargarBilletera();