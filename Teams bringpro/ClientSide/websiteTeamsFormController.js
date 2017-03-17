(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('websiteTeamsFormController', websiteTeamsFormController);

    websiteTeamsFormController.$inject = ['$scope', '$baseController', "$websiteTeamService", "$websiteService", "$routeParams", "$utilityService", "$uibModal"];

    function websiteTeamsFormController(
        $scope
        , $baseController
        , $websiteTeamService
        , $websiteService
        , $routeParams
        , $utilityService
        , $uibModal) {

        var vm = this;
        vm.teams = null;
        vm.websiteId = null;
        vm.$routeParams = $routeParams;
        vm.selectedSite = {};
        vm.selectedTeam = {};
        vm.createMode = false;
        vm.addressComponents = null;
        vm.addressId = null;
        vm.zipCodeInput = null;
        vm.zipCodes = [];
        vm.zipCodesToSend = [];
        vm.streetNumber = null;
        vm.streetName = null;
        vm.currentAddress = {};
        vm.teamId = null;
        vm.oldAddressId = null;
        vm.zipArrayIndex = null;


        console.log("slugRoute", $routeParams.Slug);
        console.log("ID FOR THE ROUTE", $routeParams.id);

        vm.selectedSite.slug = $routeParams.Slug;

        vm.$websiteTeamService = $websiteTeamService;
        vm.$websiteService = $websiteService;
        vm.$utilityService = $utilityService;
        vm.$scope = $scope;
        vm.$uibModal = $uibModal;
        vm.receiveItem = _receiveItem;
        vm.selectEmployee = _selectEmployee;
        vm.onEmpError = _onEmpError;
        vm.teamFormSubmit = _teamFormSubmit;
        vm.deleteTeam = _deleteTeam;
        vm.inputZipToArray = _inputZipToArray;
        vm.deleteZip = _deleteZip;

        $baseController.merge(vm, $baseController);

        vm.notify = vm.$websiteTeamService.getNotifier($scope);

        render();

        function render() {
            vm.$websiteService.getBySlug($routeParams.Slug, _getBySlugSuccess, _getBySlugError);
            vm.$websiteService.get(_receiveWebsites, _onWebsitesError);
            if (typeof($routeParams.id) !== "undefined") {
                vm.$websiteTeamService.getTeamById($routeParams.id, _receiveItem, _getTeamByIdError);
            }
            else {
                console.log("Create mode");
                vm.createMode = true;
            }
            
        }

        function _receiveItem(data) {
            console.log("websiteIds", vm.website);
           
            vm.notify(function () {
                vm.addressId = data.item.addressId;
                vm.zipCodes = _receiveZipCodes(data.item.zipCodes);
                vm.website = data.item.websiteIds;
                vm.selectedTeam = data.item;
                console.log("SELECTED TEAM", vm.selectedTeam);
            });
            vm.$websiteService.getAddressByExternalPlaceId(data.item.addressId, _getAddressByExternalPlaceIdSuccess, _getAddressByExternalPlaceIdError);
           
        }
        function _getTeamByIdError(err) {
            console.log(err);
        }
        function _getBySlugSuccess(data) {
            console.log("data", data);
            vm.websiteId = data.item.id;
        }
        function _getBySlugError(err) {
            console.log(err);
        }
        function _selectEmployee(anEmp) {
            console.log(anEmp);
            vm.selectedEmployee = anEmp;
        }

        function _onEmpError(jqXhr, error) {
            console.error(error);
        }

        function _teamFormSubmit() {
            console.log("address components are: ", vm.addressComponents);
            if (vm.createMode === false) {
                vm.$utilityService.getDataFromPlaceObject(vm);

                vm.selectedTeam.oldAddressId = vm.selectedTeam.addressId;
                vm.selectedTeam.addressId = vm.addressComponents.id;



                //Add the array of websiteIds and zipcodes
                
                vm.selectedTeam.websiteIds = vm.website;
                vm.selectedTeam.zipCodes = vm.zipCodesToSend;
                console.log("ZIPCODES SUBMIT!", vm.zipCodesToSend);
                console.log("Team selected!", vm.selectedTeam);
                
                
                vm.$websiteTeamService.updateTeam($routeParams.id, vm.selectedTeam, _updateNewTeamSuccess, _insertNewTeamError);
                console.log("Right before address", vm.currentAddress);
                
                
            }
            else {
                vm.$utilityService.getDataFromPlaceObject(vm);

                vm.selectedTeam.oldAddressId = vm.selectedTeam.addressId;
                vm.selectedTeam.addressId = vm.addressComponents.id;

                console.log("address id is: ", vm.addressId);
                console.log("zip is: ", vm.zipCode);

                //Add the array of websiteIds and zipcodes
                vm.selectedTeam.websiteIds = vm.website;
                vm.selectedTeam.zipCodes = vm.zipCodesToSend;

                

                vm.selectedTeam.websiteId = vm.websiteId;
                vm.$websiteTeamService.insertNewTeam(vm.selectedTeam, _insertNewTeamSuccess, _insertNewTeamError);
                vm.$websiteService.insertAddress(vm.currentAddress, _insertAddressSuccess, _insertAddressError);
            }
        }

        function _inputZipToArray(zipCode) {
            var zipObj = {};
            zipObj.zipCode = zipCode;
            zipObj.id = vm.zipCodes.length;
            vm.zipCodes.push(zipObj);
            vm.zipCodesToSend.push(zipCode);
            vm.zipCodeInput = "";
            console.log("zipArray", vm.zipCodes);
            console.log("zipToSend", vm.zipCodesToSend);
        }

        function _deleteZip(zipArrayIndex) {
            vm.zipArrayIndex = zipArrayIndex;
            _openZipModal();
        }

        function _receiveWebsites(data) {
            
                var arrayObj = [];
                $.each(data.items, function (index, value) {
                    var websiteObj = {};
                    websiteObj.id = value.id;
                    websiteObj.title = value.name;

                    arrayObj.push(websiteObj);
                })
                console.log("Websites", arrayObj);
                return vm.websites = arrayObj;
        }
        function _receiveZipCodes(data) {
            console.log("zipsHERE", data);
                var arrayObj = [];
                $.each(data, function (index, value) {
                    var zipCodeObj = {};
                    zipCodeObj.id = index;
                    zipCodeObj.zipCode = value;
                    //Add to list which keeps track of what to send
                    vm.zipCodesToSend.push(value);
                    console.log("Added some zips", vm.zipCodesToSend);


                    arrayObj.push(zipCodeObj);
                })
                console.log("ZipCodes", arrayObj);
                return vm.zipCodes = arrayObj;
        }

        function _getAddressByExternalPlaceIdSuccess(data) {
            console.log("Succesfully got address!");
            vm.notify(function () {
                vm.addressComponents = vm.$utilityService.formatAddress(data.item);
                vm.addressComponents.id = vm.addressId;
            });
            
        }
        function _getAddressByExternalPlaceIdError(err) {
            console.log("Address Get failed", err);
        }

        function _insertAddressSuccess(data) {
            console.log("Address succesfully added!", data);
            window.location.href = "backoffice#/website/" + $routeParams.Slug + "/teams";
        }

        function _updateNewTeamSuccess() {
            console.log("Update was successful!");
            vm.$websiteService.insertAddress(vm.currentAddress, _insertAddressSuccess, _insertAddressError);
        }

        function _insertNewTeamSuccess() {
            console.log("Insert was successful!");
        }

        function _deleteTeamSuccess() {
            console.log("Succesfully deleted Team!");
            
            window.location.href = "backoffice#/website/" + $routeParams.Slug + "/teams";
        }

        function _insertAddressError(err) {
            console.log(err);
        }

        function _onWebsitesError(err) {
            console.log("Error getting websites", err);
        }

        function _deleteTeamError(err) {
            console.log("Error trying to delete team", err);
        }

        function _insertNewTeamError(err) {
            console.log("ERROR trying to insert or update", err);
        }

        function _deleteTeam(teamId) {
            console.log("addressID", vm.addressId);
            vm.teamId = teamId;
            _openModal();
        }


        function _openModal() {
            var modalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: '/Assets/Themes/bringpro/js/features/backoffice/templates/websites/websiteTeamsModal.html',
                controller: 'teamsDeleteModalController as mc',
                size: 'md',
                resolve: {
                    items: function () {
                        return vm.modalItems;
                    }
                }
            });

            modalInstance.result.then(function () {
                console.log("current Id when delete happens: ", vm.teamId);
                vm.$websiteService.deleteAddressByExternalId(vm.addressId);
                vm.$websiteTeamService.deleteTeam(vm.teamId, _deleteTeamSuccess, _deleteTeamError);

            }, function () {
                console.log('Modal dismissed at: ' + new Date());
            });
        }

        function _openZipModal() {
            var modalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: '/Assets/Themes/bringpro/js/features/backoffice/templates/websites/websiteTeamsModal.html',
                controller: 'teamsDeleteModalController as mc',
                size: 'md',
                resolve: {
                    items: function () {
                        return vm.modalItems;
                    }
                }
            });

            modalInstance.result.then(function () {
                console.log("zip stuff just happened ", vm.teamId);
                vm.zipCodes.splice(vm.zipArrayIndex, 1);
                vm.zipCodesToSend.splice(vm.zipArrayIndex, 1);

            }, function () {
                console.log('Modal dismissed at: ' + new Date());
            });
        }


    }
})();