// =========================================================
// CREAR SUBASTA
// Lógica de los estados y validaciones de publicar.html.
// =========================================================

(function () {

    // -----------------------------------------------------
    // ELEMENTOS PRINCIPALES
    // -----------------------------------------------------

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


    // -----------------------------------------------------
    // CAMPOS DEL FORMULARIO
    // -----------------------------------------------------

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


    // -----------------------------------------------------
    // ESTADO LOCAL
    // -----------------------------------------------------

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


    // -----------------------------------------------------
    // VALIDACIÓN DE URL
    // -----------------------------------------------------

    function urlImagenValida(valor) {

        if (valor === "") {
            return true;
        }

        try {

            const url = new URL(valor);

            if (url.protocol !== "http:" && url.protocol !== "https:") {
                return false;
            }

            return true;

        } catch {
            return false;
        }
    }


    // -----------------------------------------------------
    // OBTENER ERROR DE UN CAMPO
    // Esta función solamente comprueba datos.
    // NO modifica visualmente el formulario.
    // -----------------------------------------------------

    function obtenerErrorCampo(nombreCampo) {

        const campo = campos[nombreCampo];
        const valor = campo.value.trim();


        if (nombreCampo === "categoria") {

            if (valor === "") {
                return "Seleccioná una categoría.";
            }
        }


        if (nombreCampo === "imagenUrl") {

            if (valor !== "" && !urlImagenValida(valor)) {
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


    // -----------------------------------------------------
    // MOSTRAR / OCULTAR ERROR
    // -----------------------------------------------------

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


    // -----------------------------------------------------
    // COMPROBAR FORMULARIO COMPLETO
    // No muestra errores.
    // Solo decide si PUBLICAR puede habilitarse.
    // -----------------------------------------------------

    function formularioValido() {

        for (const nombreCampo in campos) {

            const error = obtenerErrorCampo(nombreCampo);

            if (error !== "") {
                return false;
            }
        }

        return true;
    }


    // -----------------------------------------------------
    // BOTÓN PUBLICAR
    // -----------------------------------------------------

    function actualizarBotonPublicar() {

        const valido = formularioValido();

        if (valido && !publicando) {
            btnPublicar.disabled = false;
        } else {
            btnPublicar.disabled = true;
        }
    }


    // -----------------------------------------------------
    // PREVIEW DE IMAGEN
    // -----------------------------------------------------

    function mostrarPlaceholderImagen() {

        previewImage.style.display = "none";
        previewImage.removeAttribute("src");

        previewPlaceholder.style.display = "flex";
    }


    function actualizarPreviewImagen() {

        const url = campos.imagenUrl.value.trim();


        if (url === "") {

            mostrarPlaceholderImagen();
            return;
        }


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


    // -----------------------------------------------------
    // EVENTOS DE LOS CAMPOS
    // -----------------------------------------------------

    for (const nombreCampo in campos) {

        const campo = campos[nombreCampo];


        campo.addEventListener("input", function () {

            // Si este campo ya había sido tocado,
            // actualizamos su error mientras se corrige.
            if (camposTocados[nombreCampo]) {
                actualizarErrorCampo(nombreCampo);
            }


            // Si cambia la fecha de inicio,
            // también puede cambiar la validez de fechaFin.
            if (
                nombreCampo === "fechaInicio" &&
                camposTocados.fechaFin
            ) {
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


            if (
                nombreCampo === "fechaInicio" &&
                camposTocados.fechaFin
            ) {
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


            if (
                nombreCampo === "fechaInicio" &&
                camposTocados.fechaFin
            ) {
                actualizarErrorCampo("fechaFin");
            }


            actualizarBotonPublicar();
        });
    }


    // -----------------------------------------------------
    // LIMPIAR ERRORES
    // -----------------------------------------------------

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


    // -----------------------------------------------------
    // RESTAURAR BOTÓN PUBLICAR
    // -----------------------------------------------------

    function restaurarBotonPublicar() {

        publicando = false;

        if (iconoPublicar) {
            iconoPublicar.classList.remove("d-none");
        }

        if (spinnerPublicar) {
            spinnerPublicar.classList.add("d-none");
        }

        if (textoPublicar) {
            textoPublicar.textContent = "PUBLICAR SUBASTA";
        }

        actualizarBotonPublicar();
    }


    // -----------------------------------------------------
    // RESTAURAR FORMULARIO AL ESTADO INICIAL
    // -----------------------------------------------------

    function restaurarFormulario() {

        form.reset();

        limpiarErrores();

        mostrarPlaceholderImagen();

        restaurarBotonPublicar();
    }


    // -----------------------------------------------------
    // CANCELAR
    // -----------------------------------------------------

    btnCancelar.addEventListener("click", function () {

        restaurarFormulario();
    });


    // -----------------------------------------------------
    // CREAR OTRA SUBASTA
    // -----------------------------------------------------

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


    // -----------------------------------------------------
    // PUBLICAR SUBASTA
    // Actualmente se simula la espera.
    // La integración real con la API se hará después.
    // -----------------------------------------------------

    form.addEventListener("submit", function (evento) {

        evento.preventDefault();


        if (publicando) {
            return;
        }


        if (!formularioValido()) {
            actualizarBotonPublicar();
            return;
        }


        publicando = true;

        btnPublicar.disabled = true;


        if (iconoPublicar) {
            iconoPublicar.classList.add("d-none");
        }


        if (spinnerPublicar) {
            spinnerPublicar.classList.remove("d-none");
        }


        if (textoPublicar) {
            textoPublicar.textContent = "PUBLICANDO...";
        }


        // Simulación temporal.
        // Después se reemplazará por la llamada real al backend.
        setTimeout(function () {

            form.classList.add("d-none");

            successPanel.classList.remove("d-none");

            publicando = false;

            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });

        }, 1400);
    });


    // -----------------------------------------------------
    // ESTADO INICIAL
    // -----------------------------------------------------

    mostrarPlaceholderImagen();

    limpiarErrores();

    actualizarBotonPublicar();

})();
