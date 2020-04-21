# Quick guide for the process we use on GitHub

## Branches
### Master
`Master` is the stable branch, this has the code for the latest release in the Microsoft Store. Only pull requests from the `develop` branch will be merged in here.

### Develop
`Develop` is the development branch which includes all the latest changes from Quick Pad, beta builds will be released from this branch. Once `develop` is found to be stable then it will be merged in to `master`.

### Legacy
`Legacy` is the old code base of Quick Pad and is kept on online for reference.

### Other branches
There are other branches as well on Quick Pad, these are usually for new features or changes that are still a work in progress and not yet ready to be merged in to `develop`. These branches are temporary and will be removed once the changes are merged in to `develop`.


## Contributing to Quick Pad

To get started, create a fork of the repository on GitHub. You will need to do that to make any changes since the Quick Pad repo is restricted to a few contributors of the app. If you decide to make a change you should always do it on the `develop` branch. Once your change is completed you can open a pull request which we will review and if we like it we will merge it in 

### Related Documents
* [Refactoring](../docs/REFACTOR.md)
* [Translation](../docs/TRANSLATOR.md)
* [Code Of Conduct](../docs/CODE_OF_CONDUCT.md)

If you have any questions or feedback make sure to open an issue here.
