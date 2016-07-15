/// <reference path="../../../angular.js" />

'use strict';

(function () {

    var module = angular.module('netfusion.portal', [
        'ngMaterial',
        'netfusion.structure',
        'netfusion.plugins']);

    module.config(function ($mdThemingProvider) {
        $mdThemingProvider.theme('default')
            .primaryPalette('blue-grey')
            .accentPalette('orange');

        $mdThemingProvider.setDefaultTheme('default');
    });

})();