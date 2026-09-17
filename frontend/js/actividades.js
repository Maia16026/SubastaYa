import {
    obtenerPujasPorComprador,
    obtenerSubastasPorVendedor,
    obtenerSubastaPorId
} from "./api.js";

import { obtenerUsuarioActivo } from "./app.js";


// =========================================================
// ELEMENTOS DE LA PÁGINA
// =========================================================

const btnCompras = document.getElementById("btnCompras");
const btnPublicaciones = document.getElementById("btnPublicaciones");

const seccionCompras = document.getElementById("seccionCompras");
const seccionPublicaciones = document.getElementById("seccionPublicaciones");

const listaCompras = document.getElementById("listaCompras");
const sinCompras = document.getElementById("sinCompras");

const listaPublicaciones = document.getElementById("listaPublicaciones");
const sinPublicaciones = document.getElementById("sinPublicaciones");


// =========================================================
// FORMATO
// =========================================================

function formatearDinero(monto) {
    if (monto === null || monto === undefined) {
        return "-";
    }

    return "$" + Number(monto).toLocaleString("es-AR", {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    });
}


function formatearFecha(fecha) {
    if (!fecha) {
        return "-";
    }

    return new Date(fecha).toLocaleString("es-AR", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
    });
}


function calcularTiempoRestante(fechaFin) {
    const ahora = new Date();
    const fin = new Date(fechaFin);

    const diferencia = fin.getTime() - ahora.getTime();

    if (diferencia <= 0) {
        return "Finalizando";
    }

    const minutosTotales = Math.floor(diferencia / 60000);

    const dias = Math.floor(minutosTotales / 1440);
    const horas = Math.floor((minutosTotales % 1440) / 60);
    const minutos = minutosTotales % 60;

    if (dias > 0) {
        return `${dias} d ${horas} h`;
    }

    return `${horas} h ${minutos} min`;
}


function calcularTiempoHastaInicio(fechaInicio) {
    const ahora = new Date();
    const inicio = new Date(fechaInicio);

    const diferencia = inicio.getTime() - ahora.getTime();

    if (diferencia <= 0) {
        return "Próxima a comenzar";
    }

    const minutosTotales = Math.floor(diferencia / 60000);

    const dias = Math.floor(minutosTotales / 1440);
    const horas = Math.floor((minutosTotales % 1440) / 60);

    if (dias > 0) {
        return `Comienza en ${dias} d ${horas} h`;
    }

    return `Comienza en ${horas} h`;
}



// =========================================================
// IMAGEN DE LA SUBASTA
// =========================================================

function obtenerImagenSubasta(urlImagen) {
    if (urlImagen === "/images/phone.png") {
        return "assets/img/celular.jpg";
    }

    if (urlImagen && urlImagen.startsWith("/images/")) {
        return "assets/img/logo.png";
    }

    if (urlImagen) {
        return urlImagen;
    }

    return "assets/img/logo.png";
}

// =========================================================
// CARGA DE DATOS
// =========================================================

async function completarActividadComprador(actividad) {
    const subasta = await obtenerSubastaPorId(actividad.subastaId);

    return {
        subastaId: actividad.subastaId,
        titulo: actividad.titulo,
        imagen: obtenerImagenSubasta(subasta.urlImagen),
        estado: subasta.estado,
        sigueAbierta: actividad.sigueAbierta,
        gano: actividad.gano,
        pujaActual: subasta.pujaActual,
        cantidadPujas: subasta.cantidadPujas,
        fechaInicio: subasta.fechaInicio,
        fechaFin: subasta.fechaFin
    };
}


async function completarPublicacion(publicacion) {
    const subasta = await obtenerSubastaPorId(publicacion.subastaId);

    return {
        subastaId: publicacion.subastaId,
        titulo: publicacion.titulo,
        imagen: obtenerImagenSubasta(subasta.urlImagen),
        estado: publicacion.estado,
        precioBase: subasta.precioBase,
        incrementoMinimo: subasta.incrementoMinimo,
        fechaInicio: subasta.fechaInicio,
        fechaFin: subasta.fechaFin,
        pujaActual: publicacion.pujaActual,
        cantidadPujas: publicacion.cantidadPujas,
        montoAdjudicado: publicacion.montoAdjudicado,
        estadoAdjudicacion: publicacion.estadoAdjudicacion
    };
}


// =========================================================
// COMPRAS / PUJAS
// =========================================================

function crearDatosActividad(actividad) {
    if (actividad.estado === "ACTIVA") {
        return `
            <div class="actividad-dato">
                <span class="actividad-dato-label">
                    Puja actual
                </span>

                <span class="actividad-dato-valor">
                    ${formatearDinero(actividad.pujaActual)}
                </span>
            </div>

            <div class="actividad-dato">
                <span class="actividad-dato-label">
                    Cantidad de pujas
                </span>

                <span class="actividad-dato-valor">
                    ${actividad.cantidadPujas}
                </span>
            </div>
        `;
    }

    if (actividad.estado === "FINALIZADA") {
        return `
            <div class="actividad-dato">
                <span class="actividad-dato-label">
                    Puja ganadora
                </span>

                <span class="actividad-dato-valor">
                    ${formatearDinero(actividad.pujaActual)}
                </span>
            </div>
        `;
    }

    return `
        <div class="actividad-dato">
            <span class="actividad-dato-label">
                Puja actual
            </span>

            <span class="actividad-dato-valor">
                ${formatearDinero(actividad.pujaActual)}
            </span>
        </div>
    `;
}


function crearInformacionTiempoActividad(actividad) {
    if (actividad.sigueAbierta) {
        return `
            <div class="actividad-tiempo">

                <i class="bi bi-clock"></i>

                <div>
                    <strong>
                        ${calcularTiempoRestante(actividad.fechaFin)}
                    </strong>

                    <span>
                        tiempo restante
                    </span>
                </div>

            </div>
        `;
    }

    return `
        <div class="actividad-tiempo">

            <i class="bi bi-calendar-event"></i>

            <div>
                <span>
                    Finalizó el
                </span>

                <strong>
                    ${formatearFecha(actividad.fechaFin)}
                </strong>
            </div>

        </div>
    `;
}


function crearResultadoActividad(actividad) {
    if (actividad.sigueAbierta) {
        return `
            <div class="resultado-mensaje resultado-liderando">

                <i class="bi bi-clock-fill"></i>

                <div>
                    <strong>
                        Subasta en curso
                    </strong>

                    <span>
                        Participaste en esta subasta.
                    </span>
                </div>

            </div>
        `;
    }

    if (actividad.gano === true) {
        return `
            <div class="resultado-mensaje resultado-ganada">

                <i class="bi bi-trophy-fill"></i>

                <div>
                    <strong>
                        ¡Ganaste la subasta!
                    </strong>

                    <span>
                        Puja ganadora:
                        ${formatearDinero(actividad.pujaActual)}
                    </span>
                </div>

            </div>
        `;
    }

    if (actividad.gano === false) {
        return `
            <div class="resultado-mensaje resultado-no-ganada">

                <i class="bi bi-x-lg"></i>

                <div>
                    <strong>
                        No ganaste esta subasta
                    </strong>

                    <span>
                        Otro participante realizó la oferta ganadora.
                    </span>
                </div>

            </div>
        `;
    }

    return `
        <div class="resultado-mensaje resultado-superado">

            <i class="bi bi-hourglass-split"></i>

            <div>
                <strong>
                    Participación registrada
                </strong>

                <span>
                    La subasta todavía no tiene un resultado final.
                </span>
            </div>

        </div>
    `;
}


function obtenerClaseEstadoActividad(estado) {
    if (estado === "ACTIVA") {
        return "estado-activa";
    }

    if (estado === "PROGRAMADA") {
        return "estado-programada";
    }

    if (estado === "DESIERTA") {
        return "estado-desierta";
    }

    return "estado-finalizada";
}


function crearTarjetaActividad(actividad) {
    const tarjeta = document.createElement("article");

    tarjeta.classList.add("actividad-card");

    const claseEstado =
        obtenerClaseEstadoActividad(actividad.estado);

    tarjeta.innerHTML = `

        <div class="actividad-imagen">

            <img
                src="${actividad.imagen}"
                alt="${actividad.titulo}">

        </div>


        <div class="actividad-info">

            <h3 class="actividad-titulo">
                ${actividad.titulo}
            </h3>

            <span class="actividad-estado ${claseEstado}">
                ${actividad.estado}
            </span>

            <div class="actividad-datos">
                ${crearDatosActividad(actividad)}
            </div>

        </div>


        ${crearInformacionTiempoActividad(actividad)}


        <div class="actividad-resultado">
            ${crearResultadoActividad(actividad)}
        </div>


        <div class="actividad-boton-contenedor">

            <a
                href="sala.html?id=${actividad.subastaId}"
                class="btn-ver-subasta">

                VER SUBASTA

            </a>

        </div>
    `;

    return tarjeta;
}


function mostrarCompras(listaActividades) {
    listaCompras.innerHTML = "";

    if (listaActividades.length === 0) {
        listaCompras.classList.add("d-none");
        sinCompras.classList.remove("d-none");

        return;
    }

    listaCompras.classList.remove("d-none");
    sinCompras.classList.add("d-none");

    listaActividades.forEach(function (actividad) {
        const tarjeta = crearTarjetaActividad(actividad);

        listaCompras.appendChild(tarjeta);
    });
}


// =========================================================
// MIS PUBLICACIONES
// =========================================================

function crearDatosPublicacion(publicacion) {
    if (publicacion.estado === "PROGRAMADA") {
        return `
            <div class="actividad-dato">
                <span class="actividad-dato-label">
                    Precio base:
                </span>

                <span class="actividad-dato-valor">
                    ${formatearDinero(publicacion.precioBase)}
                </span>
            </div>

            <div class="actividad-dato">
                <span class="actividad-dato-label">
                    Incremento mínimo:
                </span>

                <span class="actividad-dato-valor">
                    ${formatearDinero(publicacion.incrementoMinimo)}
                </span>
            </div>
        `;
    }

    if (publicacion.estado === "ACTIVA") {
        return `
            <div class="actividad-dato">
                <span class="actividad-dato-label">
                    Puja actual:
                </span>

                <span class="actividad-dato-valor">
                    ${formatearDinero(publicacion.pujaActual)}
                </span>
            </div>

            <div class="actividad-dato">
                <span class="actividad-dato-label">
                    Cantidad de pujas:
                </span>

                <span class="actividad-dato-valor">
                    ${publicacion.cantidadPujas}
                </span>
            </div>
        `;
    }

    return "";
}


function crearTiempoPublicacion(publicacion) {
    if (publicacion.estado === "PROGRAMADA") {
        return `
            <div class="actividad-tiempo">

                <i class="bi bi-calendar-event"></i>

                <div>
                    <span>
                        Inicio:
                    </span>

                    <strong>
                        ${formatearFecha(publicacion.fechaInicio)}
                    </strong>

                    <span>
                        ${calcularTiempoHastaInicio(publicacion.fechaInicio)}
                    </span>
                </div>

            </div>
        `;
    }

    if (publicacion.estado === "ACTIVA") {
        return `
            <div class="actividad-tiempo">

                <i class="bi bi-clock"></i>

                <div>
                    <strong>
                        ${calcularTiempoRestante(publicacion.fechaFin)}
                    </strong>

                    <span>
                        tiempo restante
                    </span>
                </div>

            </div>
        `;
    }

    return `
        <div class="actividad-tiempo">

            <i class="bi bi-calendar-event"></i>

            <div>
                <span>
                    Finalizó el
                </span>

                <strong>
                    ${formatearFecha(publicacion.fechaFin)}
                </strong>
            </div>

        </div>
    `;
}


function crearResultadoPublicacion(publicacion) {
    if (publicacion.estadoAdjudicacion === "ADJUDICADA") {
        return `
            <div class="resultado-mensaje resultado-ganada">

                <i class="bi bi-trophy-fill"></i>

                <div>
                    <strong>
                        ADJUDICADA
                    </strong>

                    <span>
                        Recaudación:
                        ${formatearDinero(publicacion.montoAdjudicado)}
                    </span>

                    <span>
                        Se generó la venta correctamente.
                    </span>
                </div>

            </div>
        `;
    }

    if (publicacion.estadoAdjudicacion === "SIN_ADJUDICAR") {
        return `
            <div class="resultado-mensaje resultado-no-ganada">

                <i class="bi bi-x-lg"></i>

                <div>
                    <strong>
                        SIN OFERTAS
                    </strong>

                    <span>
                        La subasta finalizó sin recibir pujas.
                    </span>
                </div>

            </div>
        `;
    }

    return "";
}


function obtenerClaseEstadoPublicacion(estado) {
    if (estado === "ACTIVA") {
        return "estado-activa";
    }

    if (estado === "PROGRAMADA") {
        return "estado-programada";
    }

    if (estado === "DESIERTA") {
        return "estado-desierta";
    }

    return "estado-finalizada";
}


function crearTarjetaPublicacion(publicacion) {
    const tarjeta = document.createElement("article");

    tarjeta.classList.add("actividad-card");

    const claseEstado =
        obtenerClaseEstadoPublicacion(publicacion.estado);

    tarjeta.innerHTML = `

        <div class="actividad-imagen">

            <img
                src="${publicacion.imagen}"
                alt="${publicacion.titulo}">

        </div>


        <div class="actividad-info">

            <h3 class="actividad-titulo">
                ${publicacion.titulo}
            </h3>

            <span class="actividad-estado ${claseEstado}">
                ${publicacion.estado}
            </span>

            <div class="actividad-datos">
                ${crearDatosPublicacion(publicacion)}
            </div>

        </div>


        ${crearTiempoPublicacion(publicacion)}


        <div class="actividad-resultado">
            ${crearResultadoPublicacion(publicacion)}
        </div>


        <div class="actividad-boton-contenedor">

            <a
                href="sala.html?id=${publicacion.subastaId}"
                class="btn-ver-subasta">

                VER SUBASTA

            </a>

        </div>
    `;

    return tarjeta;
}


function mostrarPublicaciones(lista) {
    listaPublicaciones.innerHTML = "";

    if (lista.length === 0) {
        listaPublicaciones.classList.add("d-none");
        sinPublicaciones.classList.remove("d-none");

        return;
    }

    listaPublicaciones.classList.remove("d-none");
    sinPublicaciones.classList.add("d-none");

    lista.forEach(function (publicacion) {
        const tarjeta = crearTarjetaPublicacion(publicacion);

        listaPublicaciones.appendChild(tarjeta);
    });
}


// =========================================================
// ESTADOS DE CARGA Y ERROR
// =========================================================

function mostrarCargando(contenedor, mensaje) {
    contenedor.classList.remove("d-none");

    contenedor.innerHTML = `
        <div class="text-center py-5">
            <div
                class="spinner-border"
                role="status"
                aria-hidden="true">
            </div>

            <p class="mt-3 mb-0">
                ${mensaje}
            </p>
        </div>
    `;
}


function mostrarError(contenedor, mensaje) {
    contenedor.classList.remove("d-none");

    contenedor.innerHTML = `
        <div class="alert alert-danger" role="alert">
            ${mensaje}
        </div>
    `;
}


// =========================================================
// CONSULTAS AL BACKEND
// =========================================================

async function cargarCompras(usuarioId) {
    sinCompras.classList.add("d-none");

    mostrarCargando(
        listaCompras,
        "Cargando tus compras y pujas..."
    );

    try {
        const actividades =
            await obtenerPujasPorComprador(usuarioId);

        const actividadesCompletas =
            await Promise.all(
                actividades.map(completarActividadComprador)
            );

        mostrarCompras(actividadesCompletas);
    } catch (error) {
        console.error(error);

        sinCompras.classList.add("d-none");

        mostrarError(
            listaCompras,
            "No se pudieron cargar tus compras y pujas."
        );
    }
}


async function cargarPublicaciones(usuarioId) {
    sinPublicaciones.classList.add("d-none");

    mostrarCargando(
        listaPublicaciones,
        "Cargando tus publicaciones..."
    );

    try {
        const publicaciones =
            await obtenerSubastasPorVendedor(usuarioId);

        const publicacionesCompletas =
            await Promise.all(
                publicaciones.map(completarPublicacion)
            );

        mostrarPublicaciones(publicacionesCompletas);
    } catch (error) {
        console.error(error);

        sinPublicaciones.classList.add("d-none");

        mostrarError(
            listaPublicaciones,
            "No se pudieron cargar tus publicaciones."
        );
    }
}


async function cargarActividadesUsuario(usuario) {
    await Promise.all([
        cargarCompras(usuario.id),
        cargarPublicaciones(usuario.id)
    ]);
}


// =========================================================
// PESTAÑAS
// =========================================================

function mostrarPestanaCompras() {
    btnCompras.classList.add("activa");
    btnPublicaciones.classList.remove("activa");

    seccionCompras.classList.remove("d-none");
    seccionPublicaciones.classList.add("d-none");
}


function mostrarPestanaPublicaciones() {
    btnPublicaciones.classList.add("activa");
    btnCompras.classList.remove("activa");

    seccionPublicaciones.classList.remove("d-none");
    seccionCompras.classList.add("d-none");
}


btnCompras.addEventListener("click", function () {
    mostrarPestanaCompras();
});


btnPublicaciones.addEventListener("click", function () {
    mostrarPestanaPublicaciones();
});


// =========================================================
// USUARIO ACTIVO
// =========================================================

window.addEventListener("usuarioCambiado", function (evento) {
    cargarActividadesUsuario(evento.detail);
});


// =========================================================
// ESTADO INICIAL
// =========================================================

const usuarioActivo = obtenerUsuarioActivo();

mostrarPestanaCompras();
cargarActividadesUsuario(usuarioActivo);