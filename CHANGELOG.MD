# Changelog

All notable changes to this package will be documented in this file. The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)

## Unreleased [0.2.0]

### Added

* transition support
* per component navigation `IRoutable::BeforeEnter` added
* global `Router::BeforeResolve` guard added
* global `Router::AfterEach` hook added
* new samples with predefined hierarchy
* `Router.Up()` and `Router.Depth` added

### Changed

* `Router` now implemented as `ScriptableObject`
* `Router::_history` not tracking current

### Fixed

* `Router::back()` not updating currentRouteTarget

## [0.1.0] - 2022-12

### Added

* Initial release