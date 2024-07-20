<p align="center">
<h1 align="center">Pizza Box</h1>
<img alt="header image" src="https://static.poly.pizza/press/1.jpg">
</p>

## 💡 What?
Pizza box is a unity package that you can stick in your project to easily load [poly.pizza](https://poly.pizza) models at runtime. Poly pizza is a website that hosts thousands of free low poly models under CC0 and CC-BY licenses. 
You can view the [API docs here.](https://poly.pizza/docs/api)

The package also includes an example (under the samples tab in package manager) of how you might load, scale, position and generate colliders in the Unity game engine (it's harder than you think!)

Requires Unity 2019.3+

## 🖥 Install
- Make sure git is installed on your system
- [Install glTFast](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.7/manual/installation.html).
- [Install Unitask](https://github.com/Cysharp/UniTask?tab=readme-ov-file#install-via-git-url)
- Open the package manager and click "Add from Git URL"
- Paste this url `https://github.com/Chikanz/pizzabox.git`
- Get your API key from [here](https://poly.pizza/settings/api) (you'll need an account).
- Add the API manager script to your scene and paste the key in

## 🍕 Usage
The API manager provides a few methods for finding and loading models.
First you'll need to decide what models you'd like to load. There's a few methods for this:
- `GetPopular(int limit)` - Get an array of the most popular models on the site up to the `limit` you specify.

- `GetExactModel(string keyword)` - Search for a model matching your `keyword` exactly. This is useful if you know what you want to load (like an apple or a monkey) but don't know the exact model.

- `GetModelByID(string id)` - Get a model by it's unique ID. (crazy ik)

Once you've got the model data from the api you can make it into a gameObject with the `MakeModel(Model model, float scale = 1, bool positionCenter = false)` Positioning the model is tricky since the models origin can be totally fucked. Check out `spawner.cs` to see how to position objects where you want them using GLTFBounds. 

![doodleboy](https://i.imgur.com/8Kea73n.jpeg)
