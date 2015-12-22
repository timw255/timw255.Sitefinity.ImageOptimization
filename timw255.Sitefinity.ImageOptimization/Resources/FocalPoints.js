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

    this._item = null;
    this._ratioX = null;
    this._ratioY = null;

    this._focalPointX = null;
    this._focalPointY = null;

    this._dataBoundDelegate = null;
    this._previewImageClickedDelegate = null;
    this._focalPointClickedDelegate = null;
    this._showFocalPointMarkerDelegate = null;
}

timw255.Sitefinity.ImageOptimization.FocalPointsExtension.prototype = {
    initialize: function () {
        timw255.Sitefinity.ImageOptimization.FocalPointsExtension.callBaseMethod(this, "initialize");
        this._showFocalPointMarkerDelegate = Function.createDelegate(this, this._showFocalPointMarker);

        this._dataBoundDelegate = Function.createDelegate(this, this._dataBoundHandler);

        this._binder = this._detailFormView.get_binder();

        this._detailFormView.add_onDataBind(this._dataBoundDelegate);

        this._previewImage = $('.sfPreviewVideoFrame img')[0];

        this._previewImageClickedDelegate = Function.createDelegate(this, this._previewImageClicked);
        $addHandler(this._previewImage, "click", this._previewImageClickedDelegate);

        $(this._previewImage).wrap("<div id='focalPointContainer' style='display:inline-block;position:relative;'></div>");
        $('<div id="focalPoint" style="width:10px;height:10px;border-radius:10px;background:#fff;border: 2px solid #000;position:absolute;top:0;left:0;display:none;"></div>').appendTo('#focalPointContainer');

        this._focalPoint = $('#focalPoint')[0];

        this._focalPointClickedDelegate = Function.createDelegate(this, this._focalPointClicked);
        $addHandler(this._focalPoint, "click", this._focalPointClickedDelegate);

        $("<span class='sfExample'></span>").appendTo('.sfPreviewVideoFrame');
    },

    dispose: function () {
        timw255.Sitefinity.ImageOptimization.FocalPointsExtension.callBaseMethod(this, "dispose");
    },

    /* --------------------  public methods ----------- */

    /* -------------------- events -------------------- */

    /* -------------------- event handlers ------------ */
    _dataBoundHandler: function (sender, args) {
        this._item = args.Item;

        this._ratio = this._item.Width / this._previewImage.width;

        if (this._item.FocalPointX !== null && this._item.FocalPointX != 0 && this._item.FocalPointY !== null && this._item.FocalPointY != 0) {
            scaled_focalPointX = this._item.FocalPointX / this._ratio;
            scaled_focalPointY = this._item.FocalPointY / this._ratio;

            this._showFocalPointMarker(scaled_focalPointX, scaled_focalPointY);
        } else {
            this._hideFocalPointMarker();
        }
    },

    _previewImageClicked: function (sender, args) {
        var scaled_focalPointX = sender.offsetX;
        var scaled_focalPointY = sender.offsetY;

        this._focalPointX = Math.ceil(scaled_focalPointX * this._ratio);
        this._focalPointY = Math.ceil(scaled_focalPointY * this._ratio);

        this._item.FocalPointX = this._focalPointX;
        this._item.FocalPointY = this._focalPointY;

        this._showFocalPointMarker(scaled_focalPointX, scaled_focalPointY);
    },

    _focalPointClicked: function (sender, args) {
        this._item.FocalPointX = 0;
        this._item.FocalPointY = 0;

        this._hideFocalPointMarker();
    },
    /* -------------------- private methods ----------- */
    _showFocalPointMarker: function (scaled_focalPointX, scaled_focalPointY) {
        $('#focalPoint').css('left', scaled_focalPointX - 7);
        $('#focalPoint').css('top', scaled_focalPointY - 7);
        $('#focalPoint').show();

        $('.sfPreviewVideoFrame .sfExample').text('Click the focal point to remove it');
    },

    _hideFocalPointMarker: function () {
        $('#focalPoint').hide();

        $('.sfPreviewVideoFrame .sfExample').text('Click the image above to set a focal point');
    }

    /* -------------------- properties ---------------- */
}
timw255.Sitefinity.ImageOptimization.FocalPointsExtension.registerClass("timw255.Sitefinity.ImageOptimization.FocalPointsExtension", Sys.Component, Sys.IDisposable);