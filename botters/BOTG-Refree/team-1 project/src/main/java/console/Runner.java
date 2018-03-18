package console;

import console.gamerunner.*;

import java.util.Properties;

public class Runner {
    public static void main(String[] args) {

        Properties props = new Properties();
        props.put("seed", "1337");
        //Use new GameRunner(props); // if you want custom game seed.
        ConsoleGameRunner gameRunner = new ConsoleGameRunner();
        
        // ++++++++++++++++++++++++++++++++++++++++++++++++++//
        //                                                   //
        //    Comment on or off the bots you want to use     //
        //                                                   //
        // ++++++++++++++++++++++++++++++++++++++++++++++++++//

        //WAIT BOTS
        gameRunner.addAgent(SimpleWaitBot.class);
        gameRunner.addAgent(SimpleAttackBot.class);

        // Example of running another language than Java. It would be the same arguments in front of the path used in a console.

        // C# (mono is needed on a mac)
        //gameRunner.addAgent("dotnet C:\\Users\\ikolu\\sources\\botters-all\\botters\bin\\Debug\\netcoreapp2.0\\botters.dll");
        //gameRunner.addAgent("dotnet C:\\Users\\ikolu\\sources\\botters-all\\botters\bin\\Debug\\netcoreapp2.0\\botters.dll");

        gameRunner.start();
    }
}
