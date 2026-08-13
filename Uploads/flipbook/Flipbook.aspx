
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Flipbook.aspx.cs"  %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Flipbook Viewer</title>

    <!-- CSS -->
 <%--   <link href="../../flipbook/4.6.2/css/bootstrap.min.css" rel="stylesheet" />--%>
      <link href="<%= ResolveUrl("~/flipbook/4.6.2/css/bootstrap.min.css") %>" rel="stylesheet" />
    <!-- JS -->
  <script src="<%= ResolveUrl("~/js/jquery-3.6.0.min.js") %>"></script>
     <script src="<%= ResolveUrl("~/flipbook/4.6.2/js/bootstrap.bundle.min.js") %>"></script>
     <script src="<%= ResolveUrl("~/flipbook/turn.js") %>"></script>
     <script src="<%= ResolveUrl("~/flipbook/2.10.377/pdf.min.js") %>"></script>
 <%--   <script src="../../flipbook/4.6.2/js/bootstrap.bundle.min.js"></script>
    <script src="../../flipbook/turn.js"></script>
    <script src="../../flipbook/2.10.377/pdf.min.js"></script>--%>

    <style>
        html, body {
            margin: 0;
            padding: 0;
            height: 100%;
            overflow: hidden;
            background: linear-gradient(135deg, #e3e6ec, #f7f8fb);
            font-family: Arial, sans-serif;
        }

        /* Loader */
        #flipbookLoader {
            position: fixed;
            inset: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 22px;
            background: #fff;
            z-index: 1000;
        }

        /* Toolbar */
        #controls {
            position: fixed;
            top: 10px;
            left: 50%;
            transform: translateX(-50%);
            z-index: 999;
            display: flex;
            gap: 6px;
        }

        /* Container */
        #flipbook-container {
            width: 100%;
            height: 100vh;
            display: none;
            justify-content: center;
            align-items: center;
          
        }

        /* Flipbook */
        #flipbook-pages {
              width: 100vw;
             height: calc(100vh - 60px);
            max-width: 1200px;
            transform-origin: center center;
            transition: transform 0.25s ease;
             cursor: move;     /* fallback */
    cursor: grab;     /* modern browsers */
      
        }

 

@media (max-width: 768px) {
    #flipbook-pages {
        width: 100vw !important;
        height: calc(100vh - 50px) !important;
        transform: none !important;
    }
}
        #flipbook-pages.dragging {
            cursor: move;     /* fallback */
    cursor: grabbing;/* modern browsers */
  
        }

        /*.page {
            background: #fff;
            overflow: hidden;
            box-shadow: 0 15px 40px rgba(0,0,0,0.25);
        }*/
        .page {
            display: flex;
            align-items: center;
            justify-content: center;
        }

     
        canvas {
            display: block;
            margin: auto;
        }
    </style>
</head>

<body>
<form id="Form1" runat="server">
     <asp:HiddenField ID="hdnPdfName" runat="server" />
<!-- CONTROLS -->
<div id="controls">
    <button type="button" id="btnZoomIn" class="btn btn-primary btn-sm">＋</button>
    <button type="button" id="btnZoomOut" class="btn btn-primary btn-sm">－</button>
    <button type="button" id="btnZoomReset" class="btn btn-secondary btn-sm">Reset</button>
    <button type="button" id="btnAutoStart" class="btn btn-success btn-sm">▶</button>
    <button type="button" id="btnAutoStop" class="btn btn-danger btn-sm">⏸</button>
    <button type="button" id="btnFullscreen" class="btn btn-dark btn-sm" style="display:none;">⛶</button>
    <%--<button onclick="reopenFlipbook()">Reopen Flipbook</button>--%>
    <button onclick="reopenFlipbookWithoutReload()">Reopen</button>
</div>

<div id="flipbookLoader">Loading document...</div>
<div id="flipbook-container">
    <div id="flipbook-pages"></div>
</div>

</form>
<%--<script>
    $(document).ready(function () {

        /* ================= CONFIG ================= */
        var coverImageUrl = '../../flipbook/img/frontpage.jpg';
        var params = new URLSearchParams(window.location.search);
        var pdfFile = params.get("pdf");

        if (!pdfFile) {
            alert("PDF not found");
            return;
        }

        var pdfPath = '../../Modules/Legal/MergeUploadedFiles/' + pdfFile;
        var pdfDoc = null;
        var totalPages = 0;
        var renderedPages = {};
        var $book = $("#flipbook-pages");

        var zoom = 1, tx = 0, ty = 0;
        var dragging = false, startX = 0, startY = 0;

        pdfjsLib.GlobalWorkerOptions.workerSrc =
            '../../flipbook/2.10.377/pdf.worker.min.js';

        /* ================= LOAD PDF ================= */
        pdfjsLib.getDocument(pdfPath).promise.then(function (pdf) {
            pdfDoc = pdf;
            totalPages = pdf.numPages;
            buildFlipbook();
        }).catch(function () {
            alert("Error loading PDF");
        });

        /* ================= BUILD FLIPBOOK ================= */
        function buildFlipbook() {

            var isMobile = window.innerWidth <= 768;
            $book.empty();

            /* cover only for desktop */
            if (!isMobile) {
                $book.append('<div class="page cover"></div>');
            }

            for (var i = 1; i <= totalPages; i++) {
                $book.append('<div class="page"><canvas id="canvas-' + i + '"></canvas></div>');
            }

            if (!isMobile) {
                $book.append('<div class="page cover"></div>');
            }

            $("#flipbookLoader").hide();
            $("#flipbook-container").css("display", "flex");

            setTimeout(initTurn, 100);
        }

        /* ================= INIT TURN ================= */
        function initTurn() {

            var isMobile = window.innerWidth <= 768;

            var bookWidth = $("#flipbook-container").width();
            var bookHeight = window.innerHeight - 60;

            $book.turn({
                width: bookWidth,
                height: bookHeight,
                display: isMobile ? "single" : "double",
                autoCenter: true,
                gradients: true,
                elevation: 50
            });

            /* force single on mobile */
            if (isMobile) {
                $book.turn("display", "single");
            }

            for (var i = 1; i <= Math.min(6, totalPages) ; i++) {
                renderPage(i);
            }

            $book.on("turning", function (e, page) {
                for (var i = page - 1; i <= page + 1; i++) {
                    renderPage(i);
                }
            });

            $book.on("turned", function () {
                zoom = 1;
                tx = 0;
                ty = 0;
                applyTransform();
            });
        }

        /* ================= RENDER PAGE ================= */
        function renderPage(pageNum) {

            if (renderedPages[pageNum] || pageNum <= 0 || pageNum > totalPages) return;

            pdfDoc.getPage(pageNum).then(function (page) {

                var canvas = document.getElementById("canvas-" + pageNum);
                if (!canvas) return;

                var ctx = canvas.getContext("2d");
                var $page = $(canvas).parent();

                var viewport = page.getViewport({ scale: 1 });

                /* ALWAYS fit width (mobile + desktop) */
                var scale = $page.width() / viewport.width;

                var vp = page.getViewport({
                    scale: scale * Math.min(window.devicePixelRatio || 1, 2)
                });

                canvas.width = vp.width;
                canvas.height = vp.height;

                canvas.style.width = $page.width() + "px";
                canvas.style.height = "auto";

                ctx.setTransform(1, 0, 0, 1, 0, 0);
                ctx.clearRect(0, 0, canvas.width, canvas.height);

                page.render({
                    canvasContext: ctx,
                    viewport: vp
                });

                renderedPages[pageNum] = true;
            });
        }

        /* ================= ZOOM + DRAG ================= */
        function applyTransform() {
            $book.css(
                "transform",
                "translate(" + tx + "px," + ty + "px) scale(" + zoom + ")"
            );
        }

        $("#btnZoomIn").click(function () {
            zoom = Math.min(zoom + 0.2, 3);
            applyTransform();
        });

        $("#btnZoomOut").click(function () {
            zoom = Math.max(zoom - 0.2, 1);
            applyTransform();
        });

        $("#btnZoomReset").click(function () {
            zoom = 1;
            tx = 0;
            ty = 0;
            applyTransform();
        });

        $book.on("mousedown touchstart", function (e) {
            if (zoom <= 1) return;

            dragging = true;
            var p = e.touches ? e.touches[0] : e;

            startX = p.clientX - tx;
            startY = p.clientY - ty;
        });

        $(document).on("mousemove touchmove", function (e) {
            if (!dragging) return;

            var p = e.touches ? e.touches[0] : e;

            tx = p.clientX - startX;
            ty = p.clientY - startY;

            applyTransform();
        });

        $(document).on("mouseup touchend", function () {
            dragging = false;
        });

        /* ================= WHEEL ZOOM ================= */
        $book.on("wheel", function (e) {
            e.preventDefault();
            var delta = e.originalEvent.deltaY < 0 ? 0.15 : -0.15;
            zoom = Math.max(1, Math.min(zoom + delta, 3));
            applyTransform();
        });

        /* ================= PINCH ZOOM ================= */
        var pinchStart = 0;

        $book.on("touchstart", function (e) {
            if (e.touches.length === 2) {
                pinchStart = getDistance(e.touches[0], e.touches[1]);
            }
        });

        $book.on("touchmove", function (e) {
            if (e.touches.length === 2) {
                e.preventDefault();

                var dist = getDistance(e.touches[0], e.touches[1]);
                var diff = (dist - pinchStart) / 200;

                zoom = Math.max(1, Math.min(zoom + diff, 3));
                pinchStart = dist;

                applyTransform();
            }
        });

        function getDistance(t1, t2) {
            var dx = t1.clientX - t2.clientX;
            var dy = t1.clientY - t2.clientY;
            return Math.sqrt(dx * dx + dy * dy);
        }

        /* ================= FULLSCREEN ================= */
        $("#btnFullscreen").click(function () {
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen();
            } else {
                document.exitFullscreen();
            }
        });

        /* ================= RESIZE ================= */
        $(window).on("resize orientationchange", function () {
            location.reload();
        });

    });
</script>--%>
<script>
    $(document).ready(function () {
        var coverImageUrl = '../../flipbook/img/frontpage.jpg';
        var params = new URLSearchParams(window.location.search);
        var pdfFile = params.get("pdf");
        var pageRotationMap = {};
        var renderedPages = {};

       // if (renderedPages[pageNum]) return;
        if (!pdfFile) {
            alert("PDF not found");
            return;
        }

        var pdfPath = '../../Modules/Legal/MergeUploadedFiles/' + pdfFile;
        var pdfDoc = null;
        var totalPages = 0;
        var renderedPages = {};
        var $book = $("#flipbook-pages");

        pdfjsLib.GlobalWorkerOptions.workerSrc = '../../flipbook/2.10.377/pdf.worker.min.js';

        // LOAD PDF
        pdfjsLib.getDocument(pdfPath).promise.then(function(pdf) {
            pdfDoc = pdf;
            totalPages = pdf.numPages;
            buildFlipbook();
        }).catch(function(err){
            alert("Error loading PDF");
            console.error(err);
        });

        function buildFlipbook() {
            // COVER PAGE
            $book.append('<div class="page" style="background:url('+ coverImageUrl +') center/cover no-repeat; display:flex; align-items:center; justify-content:center; color:white;"> <h1>eCourt Flipbook</h1> </div>');

            // PDF PAGES
            for (var i = 1; i <= totalPages; i++) {
                $book.append('<div class="page"><canvas id="canvas-' + i + '"></canvas></div>');
            }

            // BACK COVER
            $book.append('<div class="page" style="background:url('+ coverImageUrl +') center/cover no-repeat; display:flex; align-items:center; justify-content:center; color:white;"><h2>End of Document</h2></div>');

            $("#flipbookLoader").hide();
            $("#flipbook-container").css("display", "flex");

            setTimeout(initTurn, 100);
        }

        function initTurn() {

            var isMobile = window.innerWidth <= 768;

            var containerWidth = $("#flipbook-container").width();
            var containerHeight = window.innerHeight - 70;

            var bookWidth = isMobile
                ? containerWidth
                : Math.min(containerWidth, 1200);

            var bookHeight = containerHeight;

            $book.turn({
                width: bookWidth,
                height: bookHeight,
                display: isMobile ? "single" : "double",
                autoCenter: true,
                gradients: true,
                elevation: 50
            });

            for (var i = 1; i <= Math.min(8, totalPages) ; i++) {
                renderPage(i);
            }

            $book.on("turning", function (e, page) {
                for (var i = page - 2; i <= page + 2; i++) {
                    if (!renderedPages[i]) {
                        renderPage(i);
                    }
                }
            });
            $book.on("turned", function () {
                zoom = 1;
                tx = 0;
                ty = 0;
                applyTransform();
            });
        }

        //function initTurn() {
        //    var displayMode = window.innerWidth <= 768 ? "single" : "double";
        //    $book.turn({
        //        width: $book.width(),
        //        height: $book.height(),
        //        display: displayMode,
        //        autoCenter: true,
        //        gradients: true
        //    });

        //    // Pre-render first 10 pages
        //    for (var i = 1; i <= Math.min(10, totalPages); i++) renderPage(i);

        //    $book.on("turning", function(e, page){
        //        for (var i = page-2; i <= page+2; i++) renderPage(i);
        //    });
        //}
        function renderPage(pageNum) {

            if (renderedPages[pageNum] || pageNum <= 0 || pageNum > totalPages) return;

            pdfDoc.getPage(pageNum).then(function (page) {

                var canvas = document.getElementById("canvas-" + pageNum);
                if (!canvas) return;

                var ctx = canvas.getContext("2d");
                var $page = $(canvas).parent();

                /* ================= ROTATION FIX ================= */
                if (!pageRotationMap[pageNum]) {

                    var baseRotation = page.rotate || 0;

                    baseRotation = 0;  

                    pageRotationMap[pageNum] = baseRotation;

                  //  pageRotationMap[pageNum] = baseRotation;
                }

                var rotation = pageRotationMap[pageNum];

                /* ================= VIEWPORT ================= */

                var viewport = page.getViewport({
                    scale: 1,
                    rotation: rotation
                });

                var scale = Math.min(
                    $page.width() / viewport.width,
                    $page.height() / viewport.height
                );

                var vp = page.getViewport({
                    scale: scale * Math.min(window.devicePixelRatio || 1, 1.5),
                    rotation: rotation
                });

                /* ================= CANVAS SET ================= */

                canvas.width = vp.width;
                canvas.height = vp.height;
                canvas.style.width = $page.width() + "px";
                canvas.style.height = $page.height() + "px";

                ctx.setTransform(1, 0, 0, 1, 0, 0);
                ctx.clearRect(0, 0, canvas.width, canvas.height);

                /* ================= RENDER ================= */

                page.render({
                    canvasContext: ctx,
                    viewport: vp
                }).promise.then(function () {
                    renderedPages[pageNum] = true;
                });

            });
        }
        //function renderPage(pageNum) {
        //    if (renderedPages[pageNum] || pageNum <=0 || pageNum>totalPages) return;

        //    pdfDoc.getPage(pageNum).then(function(page){
        //        var canvas = document.getElementById("canvas-" + pageNum);
        //        if (!canvas) return;

        //        var ctx = canvas.getContext("2d");
        //        var $page = $(canvas).parent();

        //        var rotation = page.rotate || 0;
        //        var viewport = page.getViewport({scale:1, rotation:rotation});
        //        var scale = Math.min($page.width()/viewport.width, $page.height()/viewport.height);
        //        var vp = page.getViewport({scale: scale * Math.min(window.devicePixelRatio || 1,1.5), rotation: rotation});
          
        //        canvas.width = vp.width;
        //        canvas.height = vp.height;
        //        canvas.style.width = $page.width() + "px";
        //        canvas.style.height = $page.height() + "px";

        //        ctx.setTransform(1,0,0,1,0,0);
        //        ctx.clearRect(0,0,canvas.width, canvas.height);

        //        page.render({canvasContext: ctx, viewport: vp}).promise.then(function(){ renderedPages[pageNum] = true; });
        //    });
        //}

        // AUTO FLIP
        var autoFlipInterval = null;
        var autoFlipDelay = 3000;

        function stopAutoFlip() {
            if(autoFlipInterval){ clearInterval(autoFlipInterval); autoFlipInterval=null; }
        }

        $("#btnAutoStart").click(function(){
            if(autoFlipInterval) return;
            autoFlipInterval = setInterval(function(){
                var current = $book.turn("page");
                var total = $book.turn("pages");
                if(current<total) $book.turn("next");
                else stopAutoFlip();
            }, autoFlipDelay);
        });
        $("#btnAutoStop").click(stopAutoFlip);

        // FULLSCREEN
        $("#btnFullscreen").click(function(){
            if(!document.fullscreenElement) document.documentElement.requestFullscreen();
            else document.exitFullscreen();
        });

        // ZOOM + DRAG
        var zoom = 1;
        var tx = 0, ty = 0;
        var dragging = false;
        var startX = 0, startY = 0;

        function applyTransform() {
            $book.css(
                "transform",
                "translate(" + tx + "px, " + ty + "px) scale(" + zoom + ")"
            );
        }

        $("#btnZoomIn").click(function () {
            zoom = Math.min(zoom + 0.2, 3);
            applyTransform();
        });

        $("#btnZoomOut").click(function () {
            zoom = Math.max(zoom - 0.2, 1);
            applyTransform();
        });

        $("#btnZoomReset").click(function () {
            zoom = 1;
            tx = 0;
            ty = 0;
            applyTransform();
        });

        $book.on("mousedown touchstart", function (e) {
            if (zoom <= 1) return;

            dragging = true;
            var p = e.touches ? e.touches[0] : e;

            startX = p.clientX - tx;
            startY = p.clientY - ty;

            $book.addClass("dragging");
        });

        $(document).on("mousemove touchmove", function (e) {
            if (!dragging) return;

            var p = e.touches ? e.touches[0] : e;

            tx = p.clientX - startX;
            ty = p.clientY - startY;

            applyTransform();
        });

        $(document).on("mouseup touchend", function () {
            dragging = false;
            $book.removeClass("dragging");
        });

        /* ================= RESPONSIVE ================= */
        $(window).on("resize orientationchange", function () {
            location.reload();
        });

    });
    function reopenFlipbook() {
        location.reload();
    }

    function reopenFlipbookWithoutReload() {

        stopAutoFlip && stopAutoFlip();

        if ($("#flipbook-pages").data("turn")) {
            $("#flipbook-pages").turn("destroy");
        }

        $("#flipbook-pages").css("transform", "");
        $("#flipbook-pages").empty();

        zoom = 1;
        tx = 0;
        ty = 0;

        renderedPages = {};

        buildFlipbook();
    }
</script>
</body>
</html>

<%--test on monday (22-12-2025)--%>
<%--<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head2" runat="server">
    <title>Flipbook Viewer</title>

    <!-- CSS -->
    <link href="../../flipbook/4.6.2/css/bootstrap.min.css" rel="stylesheet" />

    <!-- JS -->
    <script src="../../js/jquery-3.6.0.min.js"></script>
    <script src="../../flipbook/4.6.2/js/bootstrap.bundle.min.js"></script>
    <script src="../../flipbook/turn.js"></script>
    <script src="../../flipbook/2.10.377/pdf.min.js"></script>

    <style>
        html, body {
    margin: 0;
    padding: 0;
    height: 100%;
    overflow: hidden;
    font-family: Arial, sans-serif;
         /*background:
        repeating-linear-gradient(
            45deg,
            #f3f4f7,
            #f3f4f7 10px,
            #eef0f4 10px,
            #eef0f4 20px
        );*/
   background: linear-gradient(135deg, #e3e6ec, #f7f8fb);
}

        #flipbookLoader {
            position: fixed;
            inset: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 22px;
            background: #fff;
            z-index: 1000;
        }

        #flipbook-container {
            width: 100%;
            height: 100vh;
            display: none;
            justify-content: center;
            align-items: center;
            overflow: hidden;
        }

        #flipbook-pages {
            width: 95vw;
            height: 85vh;
            max-width: 1200px;
            transform-origin: center center;
            transition: transform 0.25s ease;
            cursor: move;     /* fallback */
            cursor: grab;     /* modern browsers */
        }

        #flipbook-pages.dragging {
          cursor: move;     /* fallback */
            cursor: grabbing;/* modern browsers */
        }


        .page {
            background: #fff;
            overflow: hidden;
            box-shadow: 0 10px 30px rgba(0,0,0,0.25);
        }

        .cover-text {
            /*background: #fff;*/
            overflow: hidden;
            /*box-shadow: 0 10px 30px rgba(0,0,0,0.25);*/
        }
        canvas {
            display: block;
            margin: auto;
        }
    </style>
</head>

<body>
<form id="Form2" runat="server">
    <asp:HiddenField ID="hdnPdfName" runat="server" />
<div id="controls">
    <button type="button" id="btnZoomIn" class="btn btn-primary btn-sm">＋</button>
    <button type="button" id="btnZoomOut" class="btn btn-primary btn-sm">－</button>
    <button type="button" id="btnZoomReset" class="btn btn-secondary btn-sm">Reset</button>
    <button type="button" id="btnAutoStart" class="btn btn-success btn-sm">▶</button>
    <button type="button" id="btnAutoStop" class="btn btn-danger btn-sm">⏸</button>
    <button type="button" id="btnFullscreen" class="btn btn-dark btn-sm">⛶</button>
</div>
    <div id="flipbookLoader">
        Loading document, please wait...
    </div>

    <div id="flipbook-container">
        <div id="flipbook-pages"></div>
    </div>

</form>

<script>
    $(document).ready(function () {
        debugger;
        var coverImageUrl = '../../flipbook/img/frontpage.jpg';
        var params = new URLSearchParams(window.location.search);
        var pdfFile = params.get("pdf");

        if (!pdfFile) {
            alert("PDF not found");
            return;
        }

        var pdfPath = '../../Modules/Legal/MergeUploadedFiles/' + pdfFile;

        var pdfDoc = null;
        var totalPages = 0;
        var renderedPages = {};
        var $book = $("#flipbook-pages");
        var isMobile = window.innerWidth <= 768;

        pdfjsLib.GlobalWorkerOptions.workerSrc =
            '../../flipbook/2.10.377/pdf.worker.min.js';

        // LOAD PDF
        pdfjsLib.getDocument(pdfPath).promise.then(function (pdf) {
            pdfDoc = pdf;
            totalPages = pdf.numPages;
            buildFlipbook();
        }).catch(function (err) {
            alert("Error loading PDF");
            console.error(err);
        });

        function buildFlipbook() {

            // COVER PAGE
            $book.append('<div class="page" style=" background:url('+ coverImageUrl +') center/cover no-repeat; display:flex; flex-direction:column; align-items:center; justify-content:center;  color:white;"> <h1>eCourt Flipbook</h1>  <p>Official PDF Viewer</p> </div> ');

            // PDF PAGES
            for (var i = 1; i <= totalPages; i++) {
                $book.append(
                    '<div class="page"><canvas id="canvas-' + i + '"></canvas></div>'
                );
            }
            $book.append(
       '<div class="page cover back-cover" ' +
       'style="background:url(' + coverImageUrl + ') center/cover no-repeat;  display:flex; flex-direction:column; align-items:center; justify-content:center;  color:white;">' +
       '<div class="cover-text">' +
       '<h2>End of Document</h2>' +
       '</div>' +
       '</div>'
   );
            $("#flipbookLoader").hide();
            $("#flipbook-container").css("display", "flex");

            setTimeout(initTurn, 100);
        }

        function initTurn() {

            var displayMode = window.innerWidth <= 768 ? "single" : "double";

            $book.turn({
                width: $book.width(),
                height: $book.height(),
                display: displayMode,
                autoCenter: true,
                gradients: true
            });

            // 🔥 FIRST 10 PAGES PRE-RENDER
            var preload = Math.min(10, totalPages);
            for (var i = 1; i <= preload; i++) {
                renderPage(i);
            }

            // PAGE TURN PRELOAD
            $book.on("turning", function (e, page) {
                for (var i = page - 2; i <= page + 2; i++) {
                    renderPage(i);
                }
            });
        }

        function renderPage(pageNum) {

            if (renderedPages[pageNum]) return;
            if (pageNum <= 0 || pageNum > totalPages) return;

            pdfDoc.getPage(pageNum).then(function (page) {

                var canvas = document.getElementById("canvas-" + pageNum);
                if (!canvas) return;

                var ctx = canvas.getContext("2d");
                var $page = $(canvas).parent();

                // ✅ ONLY respect PDF rotation (NO AUTO LOGIC)
                var rotation = page.rotate || 0;

                var viewport = page.getViewport({
                    scale: 1,
                    rotation: rotation
                });

                var scale = Math.min(
                    $page.width() / viewport.width,
                    $page.height() / viewport.height
                );

                var dpr = Math.min(window.devicePixelRatio || 1, 1.5);

                var vp = page.getViewport({
                    scale: scale * dpr,
                    rotation: rotation
                });

                canvas.width = vp.width;
                canvas.height = vp.height;

                canvas.style.width = $page.width() + "px";
                canvas.style.height = $page.height() + "px";

                ctx.setTransform(1, 0, 0, 1, 0, 0);
                ctx.clearRect(0, 0, canvas.width, canvas.height);

                page.render({
                    canvasContext: ctx,
                    viewport: vp
                }).promise.then(function () {
                    renderedPages[pageNum] = true;
                });
            });
        }
        var autoFlipInterval = null;
        var autoFlipDelay = 3000;


        function stopAutoFlip() {
            if (autoFlipInterval) {
                clearInterval(autoFlipInterval);
                autoFlipInterval = null;
            }
        }

        $("#btnAutoStart").click(function () {
            if (autoFlipInterval) return;

            autoFlipInterval = setInterval(function () {
                var current = $book.turn("page");
                var total = $book.turn("pages");

                if (current < total) {
                    $book.turn("next");
                } else {
                    stopAutoFlip();
                }
            }, autoFlipDelay);
        });

        $("#btnAutoStop").click(stopAutoFlip);

        // FULLSCREEN
        $("#btnFullscreen").click(function () {
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen();
            } else {
                document.exitFullscreen();
            }
        });


        // RESPONSIVE
        $(window).on("resize orientationchange", function () {
            location.reload();
        });

    });
</script>

</body>
</html>--%>
