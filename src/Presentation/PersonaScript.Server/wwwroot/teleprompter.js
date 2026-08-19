window.teleprompter = {
    scrollIntervals: {},
    activeScrolls: {},

    startScroll: function (containerId, speedWpm) {
        var element = document.getElementById(containerId);
        if (!element) return;

        this.pauseScroll(containerId);

        // Convert WPM (words per minute) or speed slider (10 to 100) to pixels per frame
        var pixelsPerFrame = (speedWpm || 30) / 25.0;
        this.activeScrolls[containerId] = true;

        var self = this;
        function step() {
            if (!self.activeScrolls[containerId]) return;

            element.scrollTop += pixelsPerFrame;

            // Check if reached bottom
            if (element.scrollTop + element.clientHeight >= element.scrollHeight - 2) {
                self.activeScrolls[containerId] = false;
                return;
            }

            requestAnimationFrame(step);
        }

        requestAnimationFrame(step);
    },

    pauseScroll: function (containerId) {
        this.activeScrolls[containerId] = false;
    },

    resetScroll: function (containerId) {
        var element = document.getElementById(containerId);
        if (element) {
            this.pauseScroll(containerId);
            element.scrollTop = 0;
        }
    },

    toggleFullscreen: function (containerId) {
        var element = document.getElementById(containerId);
        if (!element) element = document.documentElement;

        if (!document.fullscreenElement) {
            if (element.requestFullscreen) {
                element.requestFullscreen();
            } else if (element.webkitRequestFullscreen) {
                element.webkitRequestFullscreen();
            }
        } else {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            }
        }
    }
};
