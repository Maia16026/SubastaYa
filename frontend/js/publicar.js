import { crearSubasta } from "./api.js";
import { obtenerUsuarioActivo } from "./app.js";

// =========================================================
// CREAR SUBASTA
// Lógica, validaciones e integración con el backend.
// =========================================================

const form = document.getElementById("form-publicar");
const btnPublicar = document.getElementById("btnPublicar");
const btnCancelar = document.getElementById("btnCancelar");
const btnCrearOtra = document.getElementById("btnCrearOtra");
const successPanel = document.getElementById("successPanel");

const previewImage = document.getElementById("previewImage");
const previewPlaceholder = document.getElementById("previewPlaceholder");

const iconoPublicar = document.getElementById("iconoPublicar");
const textoPublicar = document.getElementById("textoPublicar");
const spinnerPublicar = document.getElementById("spinnerPublicar");

const campos = {
    categoria: document.getElementById("categoria"),
    imagenUrl: document.getElementById("imagenUrl"),
    titulo: document.getElementById("titulo"),
    descripcion: document.getElementById("descripcion"),
    precioBase: document.getElementById("precioBase"),
    incremento: document.getElementById("incremento"),
    fechaInicio: document.getElementById("fechaInicio"),
    fechaFin: document.getElementById("fechaFin")
};

const camposTocados = {
    categoria: false,
    imagenUrl: false,
    titulo: false,
    descripcion: false,
    precioBase: false,
    incremento: false,
    fechaInicio: false,
    fechaFin: false
};

let publicando = false;


// ---------------------------------------------------------
// CATEGORÍAS
// Coinciden con los IDs utilizados por el Seed y catálogo.
// ---------------------------------------------------------

function cargarCategorias() {
    const categorias = [
        { id: 1, nombre: "Tecnología" },
        { id: 2, nombre: "Coleccionables" },
        { id: 3, nombre: "Indumentaria" },
        { id: 4, nombre: "Vehículos" }
    ];

    campos.categoria.innerHTML = "";

    const opcionInicial = document.createElement("option");
    opcionInicial.value = "";
    opcionInicial.textContent = "Seleccioná una categoría";
    campos.categoria.appendChild(opcionInicial);

    categorias.forEach(function (categoria) {
        const opcion = document.createElement("option");
        opcion.value = categoria.id;
        opcion.textContent = categoria.nombre;
        campos.categoria.appendChild(opcion);
    });
}


// ---------------------------------------------------------
// VALIDACIONES
// ---------------------------------------------------------

function urlImagenValida(valor) {
    if (valor === "") {
        return false;
    }

    try {
        const url = new URL(valor);

        return url.protocol === "http:" || url.protocol === "https:";
    } catch {
        return false;
    }
}


function obtenerErrorCampo(nombreCampo) {
    const campo = campos[nombreCampo];
    const valor = campo.value.trim();

    if (nombreCampo === "categoria") {
        if (valor === "") {
            return "Seleccioná una categoría.";
        }
    }

    if (nombreCampo === "imagenUrl") {
        if (valor === "") {
            return "Ingresá la URL de la imagen.";
        }

        if (!urlImagenValida(valor)) {
            return "Ingresá una URL válida que comience con http:// o https://.";
        }
    }

    if (nombreCampo === "titulo") {
        if (valor === "") {
            return "Ingresá un título.";
        }

        if (valor.length < 5) {
            return "El título debe tener al menos 5 caracteres.";
        }
    }

    if (nombreCampo === "descripcion") {
        if (valor === "") {
            return "Ingresá una descripción.";
        }

        if (valor.length < 10) {
            return "La descripción debe tener al menos 10 caracteres.";
        }
    }

    if (nombreCampo === "precioBase") {
        const precio = Number(valor);

        if (valor === "") {
            return "Ingresá un precio base.";
        }

        if (!Number.isFinite(precio) || precio <= 0) {
            return "El precio base debe ser mayor a 0.";
        }
    }

    if (nombreCampo === "incremento") {
        const incremento = Number(valor);

        if (valor === "") {
            return "Ingresá un incremento mínimo.";
        }

        if (!Number.isFinite(incremento) || incremento <= 0) {
            return "El incremento mínimo debe ser mayor a 0.";
        }
    }

    if (nombreCampo === "fechaInicio") {
        if (valor === "") {
            return "Seleccioná la fecha de inicio.";
        }
    }

    if (nombreCampo === "fechaFin") {
        if (valor === "") {
            return "Seleccioná la fecha de finalización.";
        }

        if (campos.fechaInicio.value !== "") {
            const fechaInicio = new Date(campos.fechaInicio.value);
            const fechaFin = new Date(campos.fechaFin.value);

            if (fechaFin <= fechaInicio) {
                return "La fecha de finalización debe ser posterior a la de inicio.";
            }
        }
    }

    return "";
}


function actualizarErrorCampo(nombreCampo) {
    const campo = campos[nombreCampo];

    const mensajeError = document.querySelector(
        `.mensaje-error[data-error-for="${nombreCampo}"]`
    );

    const error = obtenerErrorCampo(nombreCampo);

    if (!camposTocados[nombreCampo]) {
        campo.classList.remove("is-invalid");

        if (mensajeError) {
            mensajeError.textContent = "";
        }

        return;
    }

    if (error !== "") {
        campo.classList.add("is-invalid");

        if (mensajeError) {
            mensajeError.textContent = error;
        }
    } else {
        campo.classList.remove("is-invalid");

        if (mensajeError) {
            mensajeError.textContent = "";
        }
    }
}


function formularioValido() {
    for (const nombreCampo in campos) {
        if (obtenerErrorCampo(nombreCampo) !== "") {
            return false;
        }
    }

    return true;
}


function actualizarBotonPublicar() {
    btnPublicar.disabled = !formularioValido() || publicando;
}


// ---------------------------------------------------------
// PREVIEW DE IMAGEN
// ---------------------------------------------------------

function mostrarPlaceholderImagen() {
    previewImage.style.display = "none";
    previewImage.removeAttribute("src");
    previewPlaceholder.style.display = "flex";
}


function actualizarPreviewImagen() {
    const url = campos.imagenUrl.value.trim();

    if (!urlImagenValida(url)) {
        mostrarPlaceholderImagen();
        return;
    }

    previewImage.onload = function () {
        previewPlaceholder.style.display = "none";
        previewImage.style.display = "block";
    };

    previewImage.onerror = function () {
        mostrarPlaceholderImagen();
    };

    previewImage.style.display = "none";
    previewPlaceholder.style.display = "flex";
    previewImage.src = url;
}


// ---------------------------------------------------------
// EVENTOS DE LOS CAMPOS
// ---------------------------------------------------------

for (const nombreCampo in campos) {
    const campo = campos[nombreCampo];

    campo.addEventListener("input", function () {
        if (camposTocados[nombreCampo]) {
            actualizarErrorCampo(nombreCampo);
        }

        if (nombreCampo === "fechaInicio" && camposTocados.fechaFin) {
            actualizarErrorCampo("fechaFin");
        }

        if (nombreCampo === "imagenUrl") {
            actualizarPreviewImagen();
        }

        actualizarBotonPublicar();
    });

    campo.addEventListener("change", function () {
        camposTocados[nombreCampo] = true;

        actualizarErrorCampo(nombreCampo);

        if (nombreCampo === "fechaInicio" && camposTocados.fechaFin) {
            actualizarErrorCampo("fechaFin");
        }

        if (nombreCampo === "imagenUrl") {
            actualizarPreviewImagen();
        }

        actualizarBotonPublicar();
    });

    campo.addEventListener("blur", function () {
        camposTocados[nombreCampo] = true;

        actualizarErrorCampo(nombreCampo);

        if (nombreCampo === "fechaInicio" && camposTocados.fechaFin) {
            actualizarErrorCampo("fechaFin");
        }

        actualizarBotonPublicar();
    });
}


// ---------------------------------------------------------
// ESTADOS DEL FORMULARIO
// ---------------------------------------------------------

function limpiarErrores() {
    for (const nombreCampo in campos) {
        camposTocados[nombreCampo] = false;

        campos[nombreCampo].classList.remove("is-invalid");

        const mensajeError = document.querySelector(
            `.mensaje-error[data-error-for="${nombreCampo}"]`
        );

        if (mensajeError) {
            mensajeError.textContent = "";
        }
    }
}


function restaurarBotonPublicar() {
    publicando = false;

    iconoPublicar?.classList.remove("d-none");
    spinnerPublicar?.classList.add("d-none");

    if (textoPublicar) {
        textoPublicar.textContent = "PUBLICAR SUBASTA";
    }

    actualizarBotonPublicar();
}


function restaurarFormulario() {
    form.reset();

    limpiarErrores();
    mostrarPlaceholderImagen();
    restaurarBotonPublicar();
}


function mostrarEstadoPublicando() {
    publicando = true;
    btnPublicar.disabled = true;

    iconoPublicar?.classList.add("d-none");
    spinnerPublicar?.classList.remove("d-none");

    if (textoPublicar) {
        textoPublicar.textContent = "PUBLICANDO...";
    }
}


// ---------------------------------------------------------
// MENSAJES DE ERROR DEL BACKEND
// ---------------------------------------------------------

function mostrarErrorPublicacion(mensaje) {
    window.alert(mensaje);
}


// ---------------------------------------------------------
// CANCELAR
// ---------------------------------------------------------

btnCancelar.addEventListener("click", function () {
    restaurarFormulario();
});


// ---------------------------------------------------------
// CREAR OTRA SUBASTA
// ---------------------------------------------------------

if (btnCrearOtra) {
    btnCrearOtra.addEventListener("click", function () {
        successPanel.classList.add("d-none");
        form.classList.remove("d-none");

        restaurarFormulario();

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    });
}


// ---------------------------------------------------------
// PUBLICAR SUBASTA EN EL BACKEND
// ---------------------------------------------------------

form.addEventListener("submit", async function (evento) {
    evento.preventDefault();

    if (publicando) {
        return;
    }

    if (!formularioValido()) {
        for (const nombreCampo in campos) {
            camposTocados[nombreCampo] = true;
            actualizarErrorCampo(nombreCampo);
        }

        actualizarBotonPublicar();
        return;
    }

    mostrarEstadoPublicando();

    try {
        const usuarioActivo = obtenerUsuarioActivo();

        const datosSubasta = {
            vendedorId: usuarioActivo.id,
            categoriaId: Number(campos.categoria.value),
            titulo: campos.titulo.value.trim(),
            descripcion: campos.descripcion.value.trim(),
            urlImagen: campos.imagenUrl.value.trim(),
            precioBase: Number(campos.precioBase.value),
            incrementoMinimo: Number(campos.incremento.value),

            // datetime-local representa la hora elegida por el usuario.
            // toISOString la convierte a UTC antes de enviarla a la API.
            fechaInicio: new Date(campos.fechaInicio.value).toISOString(),
            fechaFin: new Date(campos.fechaFin.value).toISOString()
        };

        await crearSubasta(datosSubasta);

        form.classList.add("d-none");
        successPanel.classList.remove("d-none");

        publicando = false;

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });

    } catch (error) {
        const mensaje =
            error instanceof Error
                ? error.message
                : "No se pudo publicar la subasta.";

        mostrarErrorPublicacion(mensaje);
        restaurarBotonPublicar();
    }
});


// ---------------------------------------------------------
// ESTADO INICIAL
// ---------------------------------------------------------

cargarCategorias();
mostrarPlaceholderImagen();
limpiarErrores();
actualizarBotonPublicar();
