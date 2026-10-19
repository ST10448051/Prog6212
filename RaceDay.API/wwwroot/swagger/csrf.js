
(function () {
    function configureSwagger() {
        if (!window.ui || !window.ui.getConfigs) {
            setTimeout(configureSwagger, 100);
            return;
        }

        const config = window.ui.getConfigs();

        config.requestInterceptor = async function (request) {
            const method = (request.method || "GET").toUpperCase();

            if (["POST", "PUT", "PATCH", "DELETE"].includes(method)) {
                const response = await fetch("/api/antiforgery/token", {
                    method: "GET",
                    credentials: "same-origin",
                    cache: "no-store"
                });

                if (!response.ok) {
                    throw new Error("Unable to retrieve CSRF token.");
                }

                const data = await response.json();

                request.headers = request.headers || {};
                request.headers["X-CSRF-TOKEN"] = data.token;
            }

            return request;
        };
    }

    configureSwagger();
})();

