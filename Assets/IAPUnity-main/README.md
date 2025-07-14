<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->



<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/mobile94ever/IAPUnity">
  </a>

  <h3 align="center">Unity InAppPurchasing System</h3>

</p>



<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgements">Acknowledgements</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

In App Products are a part of every mobile game and resources should not be wasted implementing the same thing each time a new project is started.
This simple yet effective module provides you an easy to access API that automatically handles the internal complications providing quick and easy
additions of new in app products.

### Built With

Following third party assests/packages were used in this framework.
* [RotaryHeartDictionary](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-lite-110992)


<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

This is an example of how to list things you need to use the software and how to install them.
* npm
  ```sh
  npm install npm@latest -g
  ```

### Installation

1. Either clone the repo or just download the unity package from the Releases(https://github.com/mobile94ever/IAPUnity/releases) section 
2. If you download the unity package be sure to install the Unity IAP plugin from the package manager (Unity 2019+)
   or you might have to install it from the services window aswel (Unity 2018 and below)



<!-- USAGE EXAMPLES -->
## Usage

*The core of the system is called the *InAppManager* which is a scriptable object. Just create an asset of it by right clicking in the project space -> Create -> ScriptableObjects -> InAppPurchasing -> InAppManager.
*Now let's say we want to create a new In App Product. The in app products are defined by another scriptable object called the *InAppItem*. Following the above procedure create an asset for that aswell. Let's say
we name it the CoinsInApp.
* Select the *InAppManager* you just created and add the product ID for the coin in app in the dictionary's key and drag and drop the relative CoinsInApp asset. That's It!
* Now hit the *Generate* button and it will create a class containing all of the product ID's in your scripts folder as specified in the dictionary's keys.

* Make sure that you initialize in app purchasing by calling the "Initialize" method of the InAppManager. You can do this in your start scene for example.
* if you want to buy an item. Just call the BuyProduct method of the InAppManager and pass in the ID from the generated script like this


* You can also subscribe to the events invoked by the InAppManager on purchase completed, faliure etc.


<!-- ROADMAP -->
## Roadmap

See the [open issues](https://github.com/mobile94ever/IAPUnity/issues) for a list of proposed features (and known issues).




<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.



<!-- CONTACT -->
## Contact

Your Name - [@your_twitter](https://twitter.com/your_username) - email@example.com

Project Link: [https://github.com/your_username/repo_name](https://github.com/your_username/repo_name)



<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements
* [RotaryHeart Serializable Dictionary](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-lite-110992)




