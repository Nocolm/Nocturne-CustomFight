# CustomFightMod
What is CustomFightMod ? It's a mod for Nocturne allowing everyone to create / import custom fight in the game for the Arcde.

# FOR KNOW THE MOD IS A BETA - THERE'S COULD BE ISSUES, BE AWARE WITH WHICH SONG YOU ARE IMPORTING 

# How works the mod ?

The mod use a `packs.json` file to store and load the customs fights, this JSON contains a **category** and a list of **directory**.

In the directory, you need at least 3 essential things, the `songinfo.json`, a `soundbank.bnk` and a `beatmap` (.ms or .txt).
You also going to need a `icon.png` for the icon of the song.

If your using custom enemy, ensure your image are in the **.png** format.
For the moment, to allow custom enemy, we need to override an existing one, so you are limited in the animation creation.

# Import a custom fight

- make sure the folder contains everythings the mod need to load it, and put it in the CustomFight folder. The in the packs.json just add what the author of it say or if nothing was say, create a category ( refere to 3 - Edit the songinfo.json and the packs.json )

# Create a custom fight

### A Template is available in the release

### 1 - The songinfo.json
  - All the datas to create a fight are inside, the most complacted ones are the soundbank and the animation.

  - Be aware to not have 2+ same songname.
  
  - The animation need an existing prefab, there is a list of prefabs tha you can use in the `PREFAB_PLACEHOLDER.txt`, all the sprites of your animations need to have the same size.

### 2 - The soundbank ( not that complicate you'll see )

You will need a .wav of your song.

  - Step 1 : Install 'Wwise Audiokinetic Launcher'
  
  - Step 2 : in the laucher at the left, search for 'Wwise' and click on it, next search at the bottom 'INSTALL A NEW VERSION', set 'Latest' to 'All', in 'Major' select 2022.1 and in 'version' select 2022.1.6.8263, then click install.
  
  - step 3 : When it's installed, click on 'Launch Wwise', make sure your launching the right version and wait, when the app open create a new project 'New...', name it, and click 'OK'. ( You don't need licence )
  
  - step 4 : go to Layouts ( in the top bar ) and select Designer or press F5. In the Project Explorer search 'Interactive Music Hierarchy' and inside it you can see 'Default Work Unit', you can keep it.
  
  - step 5 : Right Click on 'Default Work Unit' -> 'New Child' -> 'Music Playlist Container', then Right Click on 'Music Playlist Container' and select 'Import Audio Files...', click on 'Add Files...' and select your .wav , finally click on 'Import'.

  - step 5.2 : Ajust the music settings in the 'General Settings' ( sometime the 'Volume' need to be change )

  - step 6 : In the Project Explorer go to 'Events', search 'Events' and you normaly have 'Default Work Unit', Right Click on it and select 'New Child' -> 'Event'. We will create 4 main Events, START, STOP, PAUSE and RESUME.

  - step 7 : Click on your created event, then in the event tab, Right Click -> 'New Action' and select the action you need, here it's play ( also when you have multiple choice, take the basic one ). Then in the 'Target' Right Click and select 'Browse...', in the 'Interactive Music Hierarchy' search you song and select it and 'OK'

  - Repeat this process for the 3 other main events ( STOP, PAUSE and RESUME ), resume action is in the pause actions

  - step 8 : know let's create the soundbank, go to 'Layouts' and select 'SoundBank', in the 'SoundBank Manager' Tab, you have a little button with an 'S' and a '+' <img width="29" height="29" y="5" alt="image" src="https://github.com/user-attachments/assets/7594afcd-ae45-44fb-8f32-b225b9860da2" />, click on it, name your soundbank and click 'OK'.

  - step 9 : At the top select 'Views' -> 'Project Explorer' -> 'Project Explorer - New Pinned View' or Shift + E, then drag and drop the 'Deafult Work Unit' of the 'Interactive Music Hierarchy' in the 'SoundBank Editor' -> 'Add' Tab
  <img width="354" height="315" alt="image" src="https://github.com/user-attachments/assets/12485599-9253-444b-9376-19c29aeb38b9" />

  Finally, Right Click on your created soundbank -> "Generate Soundbank(s) for current platform" et-voila !

  Your soundbank will belocate to your Doc -> WwiseProject -> YOUR PROJECT -> GeneratedSoundBanks -> Windows. You just need the soundbank.bnk ( and the events ids in the soundbank.txt )

### 3 - Edit the songinfo.json and the packs.json
  - you will have do change the sounbank name and the ids with yours ( ids are in the soundbank.txt )

    <img width="655" height="111" alt="image" src="https://github.com/user-attachments/assets/3c5d63a9-1e77-45e6-b771-7161051e4c25" />
    <img width="240" height="80" alt="image" src="https://github.com/user-attachments/assets/3ea2d5df-14bc-4928-9492-cf506710ef62" />

    <img width="250" height="31" alt="image" src="https://github.com/user-attachments/assets/888c6c7b-7f46-491c-85b5-ebcdc074db9b" />

  - After you done that, in the `packs.json` you can create a category or add your directory in an existing one like this
    
    <img width="384" height="117" alt="image" src="https://github.com/user-attachments/assets/d72f6ca7-5754-4cfa-9f6a-9faa47a5e927" />
 
    
    








  
