## How to help in translating the app

### Software requirement
- Visual Studio 2017 or newer
- [Multilingual App Toolkit Extension](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308)
- [Multilingual App Toolkit Editor](https://developer.microsoft.com/en-us/windows/develop/multilingual-app-toolkit)

### Getting XLF Resource
#### Having Visual Studio with extension install
- Open Visual Studio with the Quick Pad project
- Find a "Solution Explorer" window
- Right-click on the project [Quick Pad (Universal Windows)]
- Go to "Multilingual App Toolkit" > "Add translation languages..."
- The dialog says, "Translation provider manager issue" click "OK" to ignore it.
- Select the language you want to translate by ticking the ✅ in front of that language.
- When you find the language you want, press "OK" and the extension should pull the latest text from Resource .resw for you.
- The file should reside in "Quick Pad (Universal Windows)\MultilingualResources\QuickPad.[language_code].xlf

#### Don't have access to Visual Studio
You can contact the project owner "Yair A" on Discord Yair#3380 to send you a file for a Language of your choice. Preferably, you can submit the translated file with GitHub and propose the Pull request to the project. Alternatively, send it back on Discord work too.

#### Submit a translate file into GitHub (If you don't have access to Visual Studio: Part 2)
After you have finish working with the translation, you can manually upload the file into a project, we would love to add your GitHub profile into a translator contributor as well if you do this step.
- Log into a GitHub
- Fork the project using the "Fork" button located on the top right
- Checkout to the dev branch since that is where all pull requests are merged in to.
- Open folder "Quick Pad"
- Open folder "MultilingualResources"
- On the top right of the Folder path, click "Upload files" to update your translation
- Choose the file that you finished working on
- On the "Commit changes" state the title such as "Adding translation for "Your language"" etc.
- You can put on the description too such as "Update transalation typo" etc.
- Checked "Create a new branch for this commit and start a pull request. [Learn more about pull requests.](https://help.github.com/en/articles/about-pull-requests)"
- Click "Propose changes" when you done
- Wait for GitHub to process your change
- It will redirect you to "Open a pull request" 
- Click "Create pull request" and inform developer to add it to a project

### Open the file
#### Inside Visual Studio | First time open the file
- Open Visual Studio with the Quick Pad project
- Find a "Solution Explorer" window
- Right-click on the project [Quick Pad (Universal Windows)]
- Inside the folder, \MultilingualResources the file should be in there.
- Right-click the file
- Select "Open With..."
- Select "Multilingual Editor"
- Click "Set as Default" to make Visual Studio always open that file
- Click "OK" to close the dialog and open the file

#### Inside Visual Studio | Already have Multilingual Editor as Default
If you have worked with Multilingual Editor before, and already set it as default double click the file should open the text resource in Multilingual Editor automatically.

#### Outside Visual Studio | Open from File Explorer
- Navigate to the project folder
- The XLF File should locate at \\Quick Pad\\MultilingualResources\\
- Double click a file should open the file with "Multilingual Editor" by default

#### Outside Visual Studio | Open from Multilingual Editor
- If the program is already open, you can open the file using the ribbon menu
- Click "Open"
- Navigate to the project folder
- The XLF file should locate at \Quick Pad\MultilingualResources\
- Either select the file and click "Open."
- Alternatively, double clicking the file would work as well.

### Working with the file
- You can use a translation menu to help on translating, and you can review it later if it correct. Using "Translation" > "Translate all" on Translation section
- You can use a suggestion on the current text you translating. Using "Suggest" on the Translation section
- The "State Filter" section is useful if you were working with that language before. You can uncheck "Translated" to hide all the text you already translated.
- "Source" shows the original text of it. Put your translation in the "Translation" below.
- All the text you have to translate is stored in the "Strings" tab below. You can filter it out by using "State Filter" above. The text is automatically in the "Translated" state when you write the text into the translation, "Need Reviews" is when the text from en-US source is changed. Most of the time, its probably just a fix of typo or capital letter, but you can update if the meaning of the text is different in your language. "New" state is when the translation does not translate yet.

### Q&A
#### Why do I have to translate some text more than once.
Answer: Some text have extra hotkeys added into it, for example "Open" has a tooltip with "Open (Ctrl + O)" and both of them is on a different control, One is on AppBarButton and the other is on "TextBlock" this require us to add more than one key to handle the text like "CMDOpen.Content" and "CMDOpenTooltip.Text" We'll try our best to make the text more reusable.
