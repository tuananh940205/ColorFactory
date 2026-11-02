# Coding instruction
Use action/callback when call from this view to that view, remove cross dependencies serialized fields
Use button listener by addlistener/removelistener instead of Button component
Every generated function should write explanation comments by Vietnamese
Game config and level config should be load text asset by json, then serialize to serialiable class
Text asset can be loaded by addressable by key
Data class has readonly field, set data from constructor only
1 file should define only class/object with the same name as file name

Store these objects in a singleton:
- services

Only use try-catch in functions when:
- has CancellationTokenSource

Use one static class object as service locator to store all singletons
Do not use static class or static instance for singleton items

# Unity Editor setup instruction
All popups/panels view should be packed by addressable prefab, not placed in scene at initial, shown by Instantiate Async and release when closed
Only entry scene set in scene list. The another scene will be loaded by addressables