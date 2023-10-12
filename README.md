# UI Toolkit Router

Router for Unity UI Toolkit. 

### Work in progress

For now the development is focused on current project needs but open for feedback.
Package is targeting UITK for runtime but should probably work for editor extensions as well. 

## Features

* named routes
* nested routing
* navigation guards
* transition support

## Vue Router Inspiration

The implementation is heavily inspired by vue.js router and trying to mimic the functionality documented in the [guide](https://router.vuejs.org/guide/).

Not all vue router features are as important for UI Toolkit use cases as they are for the web. 
So for everybody coming from vue.js here are some differences:

* obviously c# and not js and all the good and bad which comes with it
* only the vue router, not the framework, so no $watch for example
* only named routes, no path routing
* no \<router link> support (yet) 
* not all nav guard types

## Todo / Ideas

* cancel ongoing navigation on invoking new navigation (critical issue)
* Route meta object (idea)
* generic router for extensibility (idea)
* alternative to <string,object> params (idea)
* named views (idea)
* navigation failure object (idea)
* configure routes via ScriptableObject (idea)
* async exception handling (issue)
* global router.watch (low prio)
* per route nav guards (low prio)
