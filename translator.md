## How to help in translating the app

### Software requirement
- Visual Studio 2017 or newer
- [Multilingual App Toolkit Extension](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308)
- [Multilingual App Toolkit Editor](https://developer.microsoft.com/en-us/windows/develop/multilingual-app-toolkit)

### Getting XLF resource
#### Having Visual Studio with extension install
- Open Visual Studio with the Quick Pad project
- Find a "Solution Explorer" window
- Right click on project [Quick Pad (Universal Windows)]
- Go to "Multilingual App Toolkit" > "Add translation languages..."
- The dialog said "Translation provider manager issue" will show, click "OK" to ignore it.
- Select the language you want to translate by ticking the ✅ in front of that language.
- When you found the language you want, press "OK" and the extension will pull the latest text from Resource.resw for you.
- The file will reside in "Quick Pad (Universal Windows)\MultilingualResources\QuickPad.[language_code].xlf

#### Don't have access to Visual Studio
You can contact the project owner "Yair A" on Discord Yair#3380 to send you a file for a Language of your choice. And you can submit the translated file with github using "Upload file" menu. Or send it back on Discord.

### Open the file
#### Inside Visual Studio | First time open the file
 Open Visual Studio with the Quick Pad project
- Find a "Solution Explorer" window
- Right click on project [Quick Pad (Universal Windows)]
- Inside the folder \\MultilingualResources will store the file.
- Right click the file
- Select "Open With..."
- Select "Multilingual Editor"
- Click "Set as Default" to make Visual Studio always open that file
- Click "OK" to close the dialog and open the file

#### Inside Visual Studio | Already have Multilingual Editor as Default
If you have worked with Multilingual Editor before, and already set it as default double click the file should open the text resource in Multilinguage Editor automatically.

#### Outside Visual Studio | Open from File Explorer
- Navigate to the project folder
- The XLF File should locate at \\Quick Pad\\MultilingualResources\\
- Double click a file should open the file with "Multilingual Editor" by default

#### Outside Visual Studio | Open from Multilingual Editor
- If the program is already open you can open the file using the ribbon menu
- Click "Open"
- Navigate to the project folder
- The XLF file should locate at \\Quick Pad\\MultilingualResources\\
- Either select the file and click "Open"
- Or double click the file to open.

### Working with the file
- You can use a translation menu to help on translating and you can review it later if it correct. Using "Translation" > "Translate all" on Translation section
- You can use a suggestion on current text you translating. Using "Suggest" on Translation section
- The "State Filter" section will be useful if you working with that language before. You can uncheck "Translated" to hide all the text you already translate.
- "Source" show the original text of it. Put your translation in the "Translation" below.
- All the text you have to translate is store in "Strings" tab below. You can filter it out using "State Filter" above. The text will automatically gain "Translated" state when you write the text into translation, "Need Reviews" is when the text from en-US source is changed. Most of the time it probably fix a typo or capital letter but you can update if the meaning of the text is changed in your language. "New" state is when the translation hasn't been translate yet.
