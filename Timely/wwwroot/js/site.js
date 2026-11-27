// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// ---------------------------------------------------------------------
// Preferencias de apariencia (tema y tamaño de texto) + formularios de Mi Perfil
// El tema inicial lo aplica Views/Shared/_TemaScript.cshtml dentro de <head>
// ---------------------------------------------------------------------
(function () {
    var TEMA = 'timely-tema';     // 'claro' | 'oscuro' | 'auto'
    var TEXTO = 'timely-texto';   // 'normal' | 'grande'
    var sistemaOscuro = window.matchMedia ? window.matchMedia('(prefers-color-scheme: dark)') : null;

    function leer(clave, porDefecto) {
        try { return localStorage.getItem(clave) || porDefecto; } catch (e) { return porDefecto; }
    }
    function guardar(clave, valor) {
        try { localStorage.setItem(clave, valor); } catch (e) { }
    }
    function esOscuro(preferencia) {
        return preferencia === 'oscuro' || (preferencia === 'auto' && sistemaOscuro && sistemaOscuro.matches);
    }

    function aplicar() {
        var tema = leer(TEMA, 'auto');
        var texto = leer(TEXTO, 'normal');
        var raiz = document.documentElement;

        raiz.setAttribute('data-tema', esOscuro(tema) ? 'dark' : 'light');
        if (texto === 'grande') raiz.setAttribute('data-texto', 'grande'); else raiz.removeAttribute('data-texto');

        document.querySelectorAll('[data-tema-opcion]').forEach(function (b) {
            b.setAttribute('aria-pressed', String(b.getAttribute('data-tema-opcion') === tema));
        });
        document.querySelectorAll('[data-texto-opcion]').forEach(function (b) {
            b.setAttribute('aria-pressed', String(b.getAttribute('data-texto-opcion') === texto));
        });
        document.querySelectorAll('[data-tema-etiqueta]').forEach(function (e) {
            e.textContent = esOscuro(tema) ? 'Modo claro' : 'Modo oscuro';
        });
    }

    // Mostrar u ocultar el formulario de una tarjeta de Mi Perfil
    function mostrarFormulario(nombre, mostrar) {
        var formulario = document.querySelector('[data-perfil-form="' + nombre + '"]');
        var vista = document.querySelector('[data-perfil-vista="' + nombre + '"]');
        var boton = document.querySelector('[data-perfil-alternar="' + nombre + '"]');
        if (!formulario) return;

        formulario.hidden = !mostrar;
        if (vista) vista.hidden = mostrar;
        if (boton) boton.hidden = mostrar;

        if (mostrar) {
            var primero = formulario.querySelector('input');
            if (primero) primero.focus();
        } else {
            formulario.reset();
        }
    }

    document.addEventListener('click', function (e) {
        var opcionTema = e.target.closest('[data-tema-opcion]');
        if (opcionTema) { guardar(TEMA, opcionTema.getAttribute('data-tema-opcion')); aplicar(); return; }

        var opcionTexto = e.target.closest('[data-texto-opcion]');
        if (opcionTexto) { guardar(TEXTO, opcionTexto.getAttribute('data-texto-opcion')); aplicar(); return; }

        // Atajo del menú de usuario: alterna entre claro y oscuro
        if (e.target.closest('[data-alternar-tema]')) {
            guardar(TEMA, esOscuro(leer(TEMA, 'auto')) ? 'claro' : 'oscuro');
            aplicar();
            return;
        }

        var abrir = e.target.closest('[data-perfil-alternar]');
        if (abrir) { mostrarFormulario(abrir.getAttribute('data-perfil-alternar'), true); return; }

        var cancelar = e.target.closest('[data-perfil-cancelar]');
        if (cancelar) { mostrarFormulario(cancelar.getAttribute('data-perfil-cancelar'), false); }
    });

    // En modo "automático" el sitio sigue al dispositivo en vivo
    if (sistemaOscuro && sistemaOscuro.addEventListener) sistemaOscuro.addEventListener('change', aplicar);

    aplicar();
})();