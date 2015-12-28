// called by the DetailFormView when it is loaded
function focalPointInit(sender, args) {
    // the sender here is DetailFormView
    var detailFormView = sender;

    Sys.Application.add_init(function () {
        $create(timw255.Sitefinity.ImageOptimization.FocalPointsExtension,
        { _detailFormView: detailFormView },
        {},
        {},
        null);
    });
}

Type.registerNamespace("timw255.Sitefinity.ImageOptimization");

timw255.Sitefinity.ImageOptimization.FocalPointsExtension = function () {
    timw255.Sitefinity.ImageOptimization.FocalPointsExtension.initializeBase(this);
    // Main components
    this._detailFormView = {};
    this._binder = null;
    
    this._previewImage = null;

    this._focalCanvas = null;
    this._ctx = null;
    this._selection = {};
    this._drag = false;

    this._item = null;
    this._ratioX = null;
    this._ratioY = null;

    this._dataBoundDelegate = null;

    this._focalCanvasMouseLeaveDelegate = null;
    this._focalCanvasMouseDownDelegate = null;
    this._focalCanvasMouseUpDelegate = null;
    this._focalCanvasMouseMoveDelegate = null;
    this._previewImageLoadedDelegate = null;
}

timw255.Sitefinity.ImageOptimization.FocalPointsExtension.prototype = {
    initialize: function () {
        timw255.Sitefinity.ImageOptimization.FocalPointsExtension.callBaseMethod(this, "initialize");
        this._showFocalPointMarkerDelegate = Function.createDelegate(this, this._showFocalPointMarker);

        this._dataBoundDelegate = Function.createDelegate(this, this._dataBoundHandler);

        this._binder = this._detailFormView.get_binder();

        this._detailFormView.add_onDataBind(this._dataBoundDelegate);

        this._previewImage = $('.sfPreviewVideoFrame img')[0];

        this._previewImageLoadedDelegate = Function.createDelegate(this, this._previewImageLoaded);
        $addHandler(this._previewImage, "load", this._previewImageLoadedDelegate);

        $(this._previewImage).wrap("<div id='focalPointContainer' style='display:inline-block;position:relative;'></div>");
        $('<canvas id="focalCanvas" style="width:100%;height:100%;position:absolute;top:0px;left:0px;z-index:20;-webkit-touch-callout: none;-webkit-user-select: none;-khtml-user-select: none;-moz-user-select: none;-ms-user-select: none;user-select: none;"></canvas>').appendTo('#focalPointContainer');

        this._focalCanvas = $('#focalCanvas')[0];

        this._ctx = this._focalCanvas.getContext('2d');

        this._focalCanvasMouseDownDelegate = Function.createDelegate(this, this._focalCanvasMouseDown);
        $addHandler(this._focalCanvas, "mousedown", this._focalCanvasMouseDownDelegate);

        this._focalCanvasMouseUpDelegate = Function.createDelegate(this, this._focalCanvasMouseUp);
        $addHandler(this._focalCanvas, "mouseup", this._focalCanvasMouseUpDelegate);

        this._focalCanvasMouseMoveDelegate = Function.createDelegate(this, this._focalCanvasMouseMove);
        $addHandler(this._focalCanvas, "mousemove", this._focalCanvasMouseMoveDelegate);

        this._focalCanvasMouseLeaveDelegate = Function.createDelegate(this, this._focalCanvasMouseLeave);
        $addHandler(this._focalCanvas, "mouseleave", this._focalCanvasMouseLeaveDelegate);

        $("<span class='sfExample'></span>").appendTo('.sfPreviewVideoFrame');
    },

    dispose: function () {
        timw255.Sitefinity.ImageOptimization.FocalPointsExtension.callBaseMethod(this, "dispose");

        if (this._dataBoundDelegate) {
            delete this._dataBoundDelegate;
        }
        if (this._focalCanvasMouseLeaveDelegate) {
            delete this._focalCanvasMouseLeaveDelegate;
        }
        if (this._focalCanvasMouseDownDelegate) {
            delete this._focalCanvasMouseDownDelegate;
        }
        if (this._focalCanvasMouseUpDelegate) {
            delete this._focalCanvasMouseUpDelegate;
        }
        if (this._focalCanvasMouseMoveDelegate) {
            delete this._focalCanvasMouseMoveDelegate;
        }
        if (this._previewImageLoadedDelegate) {
            delete this._previewImageLoadedDelegate;
        }

        if (this._previewImage) {
            $removeHandler(this._previewImage, "load", this._previewImageLoadedDelegate);
        }
        if (this._focalCanvas) {
            $removeHandler(this._focalCanvas, "mousedown", this._focalCanvasMouseDownDelegate);
            $removeHandler(this._focalCanvas, "mouseup", this._focalCanvasMouseUpDelegate);
            $removeHandler(this._focalCanvas, "mousemove", this._focalCanvasMouseMoveDelegate);
            $removeHandler(this._focalCanvas, "mouseleave", this._focalCanvasMouseLeaveDelegate);
        }
    },

    /* --------------------  public methods ----------- */

    /* -------------------- events -------------------- */

    /* -------------------- event handlers ------------ */
    _dataBoundHandler: function (sender, args) {
        this._item = args.Item;

        this._ratio = this._item.Width / this._previewImage.width;
        this._clear();
    },

    _previewImageLoaded: function (sender, args) {
        this._focalCanvas.width = this._previewImage.width;
        this._focalCanvas.height = this._previewImage.height;

        if (this._item.FocalPointX !== null && this._item.FocalPointX != 0 && this._item.FocalPointY !== null && this._item.FocalPointY != 0) {
            this._selection.startX = this._item.FocalPointX / this._ratio;
            this._selection.startY = this._item.FocalPointY / this._ratio;

            this._selection.w = this._item.FocalPointWidth / this._ratio;
            this._selection.h = this._item.FocalPointHeight / this._ratio;

            this._draw();
        }
    },

    _focalCanvasMouseDown: function (sender, args) {
        var x = sender.offsetX,
            y = sender.offsetY;
        if (this._item.FocalPointX > 0 && this._item.FocalPointY > 0) {
            if (this._isPointInFocalPointMarker(x, y)) {
                this._cycleFocalPointAnchor();
                this._draw();
                return;
            } else {
                this._clearFocalPoint();
                this._clear();
            }
        }

        this._beginSelection(x, y);
        
        event.preventDefault();
    },

    _focalCanvasMouseLeave: function (sender, args) {
        this._endSelection();
    },

    _focalCanvasMouseUp: function (sender, args) {
        this._endSelection();
    },

    _focalCanvasMouseMove: function (sender, args) {
        if (this._drag) {

            var w = sender.offsetX - this._selection.startX,
                h = sender.offsetY - this._selection.startY;

            this._selection.w = w;
            this._selection.h = h;

            this._draw();
        }
    },
    /* -------------------- private methods ----------- */
    _cycleFocalPointAnchor: function () {
        p = this._item.FocalPointAnchor + 1;
        if (p > 4) {
            p = 0;
        }
        this._setFocalPointAnchor(p);
    },

    _setFocalPoint: function (x, y, w, h) {
        this._item.FocalPointX = x;
        this._item.FocalPointY = y;
        this._item.FocalPointWidth = w;
        this._item.FocalPointHeight = h;
    },

    _setFocalPointAnchor: function (p) {
        this._item.FocalPointAnchor = p;
    },

    _clearFocalPoint: function () {
        this._setFocalPoint(0, 0, 0, 0);
        this._setFocalPointAnchor(0);
        this._selection = {};
    },

    _beginSelection: function (x, y) {
        this._selection.startX = x;
        this._selection.startY = y;
        this._drag = true;
    },

    _endSelection: function () {
        this._drag = false;

        if (Math.abs(this._selection.w) < 25 || Math.abs(this._selection.h) < 25) {
            this._clearFocalPoint();
            this._clear();
            return;
        }

        var x = Math.ceil(this._selection.startX * this._ratio),
            y = Math.ceil(this._selection.startY * this._ratio),
            w = Math.ceil(this._selection.w * this._ratio),
            h = Math.ceil(this._selection.h * this._ratio);

        this._setFocalPoint(x, y, w, h);
        this._draw();
    },

    _isPointInFocalPointMarker: function (x, y) {
        var isCollision = false;

        var left = this._selection.startX,
            right = this._selection.startX + this._selection.w;
        var top = this._selection.startY,
            bottom = this._selection.startY + this._selection.h;
        if (right >= x
            && left <= x
            && bottom >= y
            && top <= y) {
            isCollision = true;
        }

        return isCollision;
    },

    _draw: function () {
        this._clear();
        this._drawFocalPoint();
        this._drawFocalPointAnchor();
    },

    _clear: function() {
        this._ctx.clearRect(0, 0, this._focalCanvas.width, this._focalCanvas.height);
    },

    _drawFocalPoint: function () {
        this._ctx.beginPath();
        this._ctx.globalAlpha = 0.5;
        this._ctx.rect(this._selection.startX, this._selection.startY, this._selection.w, this._selection.h);
        this._ctx.strokeStyle = 'rgba(0,0,0,0.9)';
        this._ctx.fillStyle = 'rgba(255,255,255,0.6)';
        this._ctx.lineWidth = 2;
        this._ctx.fill();
        this._ctx.stroke();
    },

    _drawFocalPointAnchor: function () {
        for (i = 0; i <= 4; i++) {
            var x, y;

            switch (i) {
                case 0:
                    x = this._selection.startX + (this._selection.w / 2);
                    y = this._selection.startY + (this._selection.h / 2);
                    break;
                case 1:
                    x = this._selection.startX + (this._selection.w / 2);
                    y = this._selection.startY + 10;
                    break;
                case 2:
                    x = this._selection.startX + (this._selection.w - 10);
                    y = this._selection.startY + (this._selection.h / 2);
                    break;
                case 3:
                    x = this._selection.startX + (this._selection.w / 2);
                    y = this._selection.startY + (this._selection.h - 10);
                    break;
                case 4:
                    x = this._selection.startX + 10;
                    y = this._selection.startY + (this._selection.h / 2);
                    break;
            }

            this._ctx.beginPath();
            this._ctx.arc(x, y, 3, 0, 2 * Math.PI, false);

            if (i == this._item.FocalPointAnchor) {
                this._ctx.fillStyle = 'white';
            } else {
                this._ctx.fillStyle = 'black';
            }

            this._ctx.fill();
            this._ctx.lineWidth = 1;
            this._ctx.strokeStyle = '#000000';
            this._ctx.stroke();
        }
    }
    /* -------------------- properties ---------------- */
}
timw255.Sitefinity.ImageOptimization.FocalPointsExtension.registerClass("timw255.Sitefinity.ImageOptimization.FocalPointsExtension", Sys.Component, Sys.IDisposable);