<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>ChiaReporting WebServer - Status</title>
    <style>
        body {
            font-family: 'Segoe UI', sans-serif;
            background-color: #1e1e2f;
            color: #e0e0e0;
            padding: 40px;
        }
        .container {
            max-width: 800px;
            margin: auto;
            background: #2a2a40;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 0 20px #000;
        }
        h1 {
            color: #90ee90;
            border-bottom: 1px solid #444;
            padding-bottom: 10px;
        }
        .status {
            margin: 15px 0;
            padding: 10px;
            background-color: #3a3a55;
            border-left: 6px solid #90ee90;
        }
        .fail {
            border-left-color: #f44336;
            background-color: #552a2a;
        }
        .footer {
            margin-top: 20px;
            font-size: 0.9em;
            color: #888;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>ChiaReporting Web Server Status</h1>

        <?php
        function testFunction($name, $result) {
            $class = $result ? 'status' : 'status fail';
            $message = $result ? 'OK' : 'FAILED';
            echo "<div class=\"$class\"><strong>$name:</strong> $message</div>";
        }

        testFunction("PHP Version ≥ 7.0", version_compare(PHP_VERSION, '7.0.0', '>='));
        testFunction("cURL Installed", function_exists('curl_version'));
        testFunction("JSON Support", function_exists('json_encode'));
        testFunction("File Write Access", is_writable(__DIR__));
        ?>

        <div class="footer">
            Powered by PHP <?= phpversion(); ?>
        </div>
    </div>

    <br>
    <br>
    <br>
    <br>
    <br>
    <hr>
    <?php phpinfo(); ?>

</body>
</html>









