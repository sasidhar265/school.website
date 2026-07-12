window.schoolConnectPainting = {
    initialize(canvasId, colourId, sizeId, templateUrl) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || canvas.dataset.ready === "true") return;

        const context = canvas.getContext("2d");
        const resize = () => {
            canvas.width = Math.max(320, Math.floor(canvas.clientWidth));
            canvas.height = Math.max(320, Math.floor(canvas.clientHeight));
        };
        resize();
        this.loadTemplate(canvasId, templateUrl);

        let drawing = false;
        const point = event => {
            const rect = canvas.getBoundingClientRect();
            return { x: event.clientX - rect.left, y: event.clientY - rect.top };
        };
        canvas.addEventListener("pointerdown", event => {
            drawing = true;
            const p = point(event);
            context.beginPath();
            context.moveTo(p.x, p.y);
            canvas.setPointerCapture(event.pointerId);
        });
        canvas.addEventListener("pointermove", event => {
            if (!drawing) return;
            const p = point(event);
            context.strokeStyle = document.getElementById(colourId)?.value || "#0f766e";
            context.lineWidth = Number(document.getElementById(sizeId)?.value || 7);
            context.lineCap = "round";
            context.lineJoin = "round";
            context.lineTo(p.x, p.y);
            context.stroke();
        });
        canvas.addEventListener("pointerup", () => drawing = false);
        canvas.addEventListener("pointercancel", () => drawing = false);
        canvas.dataset.ready = "true";
    },
    loadTemplate(canvasId, templateUrl) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !templateUrl) return;
        const context = canvas.getContext("2d");
        const image = new Image();
        image.onload = () => {
            context.fillStyle = "#ffffff";
            context.fillRect(0, 0, canvas.width, canvas.height);
            const scale = Math.min(canvas.width / image.width, canvas.height / image.height);
            const width = image.width * scale;
            const height = image.height * scale;
            context.drawImage(image, (canvas.width - width) / 2, (canvas.height - height) / 2, width, height);
            canvas.dataset.templateUrl = templateUrl;
        };
        image.src = templateUrl;
    },
    clear(canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        this.loadTemplate(canvasId, canvas.dataset.templateUrl);
    }
};
