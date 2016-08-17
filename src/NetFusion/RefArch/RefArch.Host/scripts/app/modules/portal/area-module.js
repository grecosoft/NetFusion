/// <reference path="../../../angular.js" />

'use strict';

(function () {

    var module = angular.module('netfusion.portal', [
        'ngMaterial',
        'ui.router',
        'netfusion.structure',
        'netfusion.plugins']);

    module.config(['$mdThemingProvider', function ($mdThemingProvider) {
        $mdThemingProvider.theme('default')
            .primaryPalette('blue-grey')
            .accentPalette('orange');

        $mdThemingProvider.setDefaultTheme('default');
    }]);

    module.config(['$locationProvider', function ($locationProvider) {
        $locationProvider.html5Mode(true);
    }]);

    module.config(['$stateProvider', function ($stateProvider) {
        $stateProvider
            .state('about', {
                url: '/about',
                templateUrl: 'scripts/app/views/portal/about.html',
                controller: 'AboutController'
            })

            .state('about2', {
                url: '/about2',
                templateUrl: 'scripts/app/views/portal/about2.html',
                controller: 'AboutController'
            });
    }]);

})();