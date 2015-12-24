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
    this.ctx = null;
    this._rect = {};
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
        $('<canvas id="focalCanvas" style="width:100%;height:100%;position:absolute;top:0px;left:0px;z-index:20;"></canvas>').appendTo('#focalPointContainer');

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
    },

    _previewImageLoaded: function (sender, args) {
        this._focalCanvas.width = this._previewImage.width;
        this._focalCanvas.height = this._previewImage.height;

        this._ctx.clearRect(0, 0, this._focalCanvas.width, this._focalCanvas.height);

        if (this._item.FocalPointX !== null && this._item.FocalPointX != 0 && this._item.FocalPointY !== null && this._item.FocalPointY != 0) {
            this._rect.startX = this._item.FocalPointX / this._ratio;
            this._rect.startY = this._item.FocalPointY / this._ratio;

            this._rect.w = this._item.FocalPointWidth / this._ratio;
            this._rect.h = this._item.FocalPointHeight / this._ratio;

            this._drawFocalMarker();
        }
    },

    _focalCanvasMouseDown: function (sender, args) {
        this._rect.startX = sender.offsetX;
        this._rect.startY = sender.offsetY;
        this._drag = true;
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
            this._rect.w = sender.offsetX - this._rect.startX;
            this._rect.h = sender.offsetY - this._rect.startY;
            
            this._drawFocalMarker();
        }
    },
    /* -------------------- private methods ----------- */
    _endSelection: function () {
        this._drag = false;

        if (Math.abs(this._rect.w) < 25 || Math.abs(this._rect.h) < 25) {
            this._ctx.clearRect(0, 0, this._focalCanvas.width, this._focalCanvas.height);

            this._item.FocalPointX = 0;
            this._item.FocalPointY = 0;

            this._item.FocalPointWidth = 0;
            this._item.FocalPointHeight = 0;

            $('.sfPreviewVideoFrame .sfExample').text('Click and drag on the image above to set a focal point');

            return;
        }

        if (this._rect.w < 0 || this._rect.h < 0) {
            this._item.FocalPointX = Math.ceil((this._rect.startX + this._rect.w) * this._ratio);
            this._item.FocalPointY = Math.ceil((this._rect.startY + this._rect.h) * this._ratio);
        } else {
            this._item.FocalPointX = Math.ceil(this._rect.startX * this._ratio);
            this._item.FocalPointY = Math.ceil(this._rect.startY * this._ratio);
        }

        this._item.FocalPointWidth = Math.abs(Math.ceil(this._rect.w * this._ratio));
        this._item.FocalPointHeight = Math.abs(Math.ceil(this._rect.h * this._ratio));

        $('.sfPreviewVideoFrame .sfExample').text('Click the focal point to remove it');
    },

    _drawFocalMarker: function () {
        this._ctx.clearRect(0, 0, this._focalCanvas.width, this._focalCanvas.height);
        this._ctx.beginPath();
        this._ctx.globalAlpha = 0.5;
        this._ctx.rect(this._rect.startX, this._rect.startY, this._rect.w, this._rect.h);
        this._ctx.strokeStyle = 'rgba(0,0,0,0.9)';
        this._ctx.fillStyle = 'rgba(255,255,255,0.6)';
        this._ctx.lineWidth = 2;
        this._ctx.fill();
        this._ctx.stroke();
    },
    /* -------------------- properties ---------------- */
}
timw255.Sitefinity.ImageOptimization.FocalPointsExtension.registerClass("timw255.Sitefinity.ImageOptimization.FocalPointsExtension", Sys.Component, Sys.IDisposable);