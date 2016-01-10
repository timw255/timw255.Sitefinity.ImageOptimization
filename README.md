# timw255.Sitefinity.ImageOptimization

This is a custom module that adds functionality for users to optimize images within Sitefinity albums.

[See this module in action on YouTube (1:20)](https://www.youtube.com/watch?v=jG3Vy_I58bs)

## Features

Batch optimization of images in Sitefinity albums

"One off" image processing

Extendable OptimizerBase class so developers can create custom image optimizers

Optimization with either ImageMagick (default) or Kraken.io

Focal Points
[See Focal Points in action on YouTube (1:16)](https://www.youtube.com/watch?v=IPIg6ddblU8)

Thumbnail generation based on focal points

###Changing the settings
The default image optimizers can be configured by navigating to _(Administration -> Settings -> Advanced -> ImageOptimization -> Optimizers)_.

By default, the ImageMagick optimizer is set to 85/100 quality. To change the compression level, modify the "imageQuality" parameter.

###If you'd like to use the Kraken.io image optimizer
* Head over to [Kraken.io](https://kraken.io/) and register for an account.
* Enter your API Key and API Secret in the new "KrakenImageOptimizer" configuration section. _(Administration -> Settings -> Advanced -> ImageOptimization -> Optimizers -> KrakenImageOptimizer -> Parameters)_

**Note:** By default **Lossy optimization** is turned off but it's worth turning on if you want to really optimize the images. Check out [the docs](https://kraken.io/docs/lossy-optimization) to see if it's something you're interested in.

###It should go without saying but...
Please backup your site (including the database) before installing or using this module. The actual image data is overwritten when it gets optimized. (That means there is no "undo".)
