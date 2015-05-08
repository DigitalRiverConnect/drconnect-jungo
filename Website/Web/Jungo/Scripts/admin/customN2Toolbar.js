/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="../N2CMS/src/Mvc/MvcTemplates/N2/Resources/angular-1.1.5/angular.js" />

(function (DR) {
    DR.constant('designState', {
        deployed: 'Deployed',
        design: 'Design',
        designOrDeployed: 'DesignOrDeployed'
    });

    DR.constant('productDesignModeCookie', 'productDesignMode');
})(angular.module('n2-custom-toolbar', ['ngCookies']));

var DR;

(function (DR) {
    DR.ProductDesignModeController = function ($scope, $cookieStore, designState, productDesignModeCookie) {
        $scope.clicked = function () {
            var productDesignMode = getCookie(productDesignModeCookie);

            if (productDesignMode != null && productDesignMode != '') {
                // Make it only two-state for now
                if (productDesignMode == designState.designOrDeployed) {
                    productDesignMode = designState.deployed;
                } else {
                    productDesignMode = designState.designOrDeployed;
                }
            } else {
                productDesignMode = designState.designOrDeployed;
            }

            setCookie(productDesignModeCookie, productDesignMode, null, '/');

            window.location.reload();
        };

        var productDesignMode = getCookie(productDesignModeCookie);

        if (productDesignMode == null || productDesignMode == "" || productDesignMode == designState.deployed) {
            $scope.item.Current.Title = 'Deployed Products';
        } else {
            $scope.item.Current.Title = 'Design & Deployed Products';
        }
    };

    var setCookie = function (cName, value, exdays, path) {
        var exdate = new Date();
        exdate.setDate(exdate.getDate() + exdays);
        var cValue = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
        if (path)
            cValue = cValue + ";path=" + path;
        document.cookie = cName + "=" + cValue;
    };

    var getCookie = function (cName) {
        var cValue = document.cookie;
        var cStart = cValue.indexOf(" " + cName + "=");
        if (cStart == -1) {
            cStart = cValue.indexOf(cName + "=");
        }
        if (cStart == -1) {
            cValue = null;
        } else {
            cStart = cValue.indexOf("=", cStart) + 1;
            var cEnd = cValue.indexOf(";", cStart);
            if (cEnd == -1) {
                cEnd = cValue.length;
            }
            cValue = unescape(cValue.substring(cStart, cEnd));
        }
        return cValue;
    };
})(DR || (DR = {}));