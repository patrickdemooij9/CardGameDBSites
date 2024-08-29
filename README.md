This codebase is used to run the following websites:

- SkytearHordeDB.com
- SW-Unlimited-DB.com
- Singularity-DB.com
- GrimpathDB.com
- ShatterpointDB.com

They all run on a single machine with multi-tenant capabilities so that each website has their own section and is independent from the other sites.

The technologies used are a bit of mix of various things while I tried to get a feel on how I wanted the project to take shape. The project started out for just a single website, but then grew into something bigger and more generic than I initially thought about.

- Umbraco (CMS, Used for managing the cards in an easy to use interface.)
- NET Core
- TailwindCSS (There is also still some static CSS, but that should eventually be replaced by Tailwind)

There are various javascript things going on as I am unsure what the best way forward is.
- Main javascript functionalities is done through the plain javascript code in SkytearHordeDB.Static
- The whole deckbuilder has been created in Vue.
- Some interactivity still uses AlpineJS, but I don't plan on using that a lot more.
- I have started to move some things towards Blazor, but I am not sure if that is were I want everything to be at.
Let me know if you have some tips about this. I would highly appreciate it. I mainly want a serverside rendered website that adds a bit of javascript functionality.

I'll be working on getting some test data set up, so that this project can easily be used for other things. Currently I have a database running with everything manually set up.
