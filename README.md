# UIToolkit Router

Router for Unity UI Toolkit. 

### Work in progress

For now the development is focused on current project needs but open for feedback.
Package is targeting UITK for runtime but should probably work for editor extensions as well. 

## Features

* named routes
* navigation guards
* no nested routes yet

## Vue Router Inspiration

The implementation is heavily inspired by vue.js router and trying to mimic the functionality documented in the [guide](https://router.vuejs.org/guide/).

Not all vue router features are as important for UIToolkit use cases as they are for the web. 
So for everybody coming from vue.js here are some differences:

* obviously c# and not js and all the good and bad which comes with it
* only the vue router, not the framework, so no $watch for example
* purely named routes, no path routing
* no integrated transitions
* no \<router link> support (yet) 
* not all nav guard types

## Todo / Ideas

* nesting (crucial)
* more per component nav guards (planned)
* generic router for extensibility (idea)
* reusing loaded components (planned)
* more guard types like beforeResolve or per route (low prio)
* Route meta object (idea)
* global router.watch (idea)
* alternative to string params (idea)
