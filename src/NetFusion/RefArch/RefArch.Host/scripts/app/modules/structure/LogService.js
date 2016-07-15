/// <reference path="../../../angular.js" />

'use strict';

(function () {

    var module = angular.module('netfusion.structure');

    var compositeLogService = ['$http', function ($http) {

        this.getCompositeLog = function () {
            return $http({
                method: 'GET',
                url: 'api/config/composite/log'
            }).then(function (response) {
                return response.data;
            });
        };

    }];

    module.service('CompositeLogService', compositeLogService);

})();