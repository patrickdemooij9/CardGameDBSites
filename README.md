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

## How to set up
Requirements: Net Core SDK 8

- Clone the repository
- Inside of SkytearHordeDB.Website run `dotnet build` & `dotnet run`. This will start running the website.
- Navigate in your browser to the https url that is displayed in your terminal.
- Install Umbraco by filling in the fields. Ideally you use SQL server, but SQL Lite will also work (though there may be parts broken as it wasn't tested with it in mind).
- After installing, log in. If the page doesn't automatically redirect you, go to the same url but with /umbraco behind it.
- In the top navigation, go to "Settings". Then in the sidebar locate "uSync" and click on it. Now click on the Import button.
- Now stop the website and find the "ContentMigrationPlan.cs" file located in the "Content" folder. Open it and uncomment line 15. Save and run the site again.
- Go back to umbraco, navigate to "Content". you should now see an item called "Homepage". Click on there.
- Expand the "Save and publish" button to select "Publish with descendants" and toggle "Include unpublished content items" to on. Click "Publish with descendants".
- Right click on "Homepage" and select "Cultures and Hostnames". Add a new domain and put your current domain into that input. Click save.
- You should now be able to navigate to your domain (without /umbraco) and navigate through the website. You'll miss images but I'll figure out a way to get those working as well.
