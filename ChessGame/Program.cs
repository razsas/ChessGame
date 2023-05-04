namespace Chess
{
    internal class ChessGameLauncher
    {
        static void Main(string[] args)
        {
            new ChessGame().play();
        }
    }
    class Piece
    {
        int xCordinate, yCordinate;
        bool black, mooved;    //mayby move (king Pawn Rook only);
        public Piece(int x, int y, bool black)
        {
            this.mooved = false;
            this.black = black;
            this.xCordinate = x;
            this.yCordinate = y;
        }
        public bool isBlack()
        {
            return black;
        }
        public int getX()
        {
            return xCordinate;
        }
        public int getY()
        {
            return yCordinate;
        }
        public void setX(int x)
        {
            this.xCordinate = x;
        }
        public void setY(int y)
        {
            this.yCordinate = y;
        }
        public bool isMooved()
        {
            return mooved;
        }
        public void movePiece()
        {
            this.mooved = true;
        }
        public virtual bool isLegalMove(int x, int y, Board board)
        {
            return true;
        }
    }
    class Board  //one day unify with game
    {
        Piece[,] board;
        int turnsCountFor50MovesCount, blackKingX, blackKingY, whiteKingX, whiteKingY, turnCount;
        string boardPos;
        string[] blackBoardHistory, whiteBoardHistory;
        public Board()
        {
            board = new Piece[8, 8];
            blackKingX = 7;
            blackKingY = whiteKingY = 4;
            whiteKingX = 0;
            turnsCountFor50MovesCount = 0;
            blackBoardHistory = new string[300];
            whiteBoardHistory = new string[300];
            turnCount = -1;
            for (int i = 0; i < 8; i++)
            {
                board[1, i] = new Pawn(1, i, false);
                board[6, i] = new Pawn(6, i, true);
            }
            for (int i = 0; i < 2; i++)
            {
                board[0, 7 * i] = new Rook(0, 7 * i, false);
                board[7, 7 * i] = new Rook(7, 7 * i, true);
                board[0, 1 + 5 * i] = new Knight(0, 1 + 5 * i, false);
                board[7, 1 + 5 * i] = new Knight(7, 1 + 5 * i, true);
                board[0, 2 + 3 * i] = new Bishop(0, 2 + 3 * i, false);
                board[7, 2 + 3 * i] = new Bishop(7, 2 + 3 * i, true);
            }
            board[0, 3] = new Queen(0, 3, false);
            board[0, 4] = new King(0, 4, false);
            board[7, 3] = new Queen(7, 3, true);
            board[7, 4] = new King(7, 4, true);
        }
        public Piece[,] getBoard()
        {
            return this.board;
        }
        public int getTurnsCountFor50MovesCount()
        {
            return turnsCountFor50MovesCount;
        }
        public bool isPieceBlack(int x, int y)
        {
            return board[x, y].isBlack();
        }
        public void printBoard() // override everyone ToString() to make it pretty
        {
            Console.WriteLine("    A   B   C   D   E   F   G   H");
            Console.WriteLine("  ┌───┬───┬───┬───┬───┬───┬───┬───┐");
            for (int i = 0; i < 8; i++)
            {
                Console.Write((1 + i) + " │");
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] is King)
                        Console.Write(" {0}K|", board[i, j].isBlack() ? "B" : "W");
                    else if (board[i, j] is Queen)
                        Console.Write(" {0}Q|", board[i, j].isBlack() ? "B" : "W");
                    else if (board[i, j] is Rook)
                        Console.Write(" {0}R|", board[i, j].isBlack() ? "B" : "W");
                    else if (board[i, j] is Knight)
                        Console.Write(" {0}N|", board[i, j].isBlack() ? "B" : "W");
                    else if (board[i, j] is Bishop)
                        Console.Write(" {0}B|", board[i, j].isBlack() ? "B" : "W");
                    else if (board[i, j] is Pawn)
                        Console.Write(" {0}P|", board[i, j].isBlack() ? "B" : "W");
                    else
                        Console.Write("   │");
                }
                Console.Write((1 + i) + "\n  ");
                if (i < 7)
                    Console.WriteLine("├───┼───┼───┼───┼───┼───┼───┼───┤");
            }
            Console.WriteLine("└───┴───┴───┴───┴───┴───┴───┴───┘");
            Console.WriteLine("    A   B   C   D   E   F   G   H");
        }
        public void movePiece(Piece piece, int x, int y)
        {
            this.board[piece.getX(), piece.getY()] = null;
            piece.setX(x);
            piece.setY(y);
            this.board[x, y] = piece;
            // promotion
            if (piece.isBlack() && x == 0 && piece is Pawn)
                ((Pawn)piece).promotion(this);
            else if (!piece.isBlack() && x == 7 && piece is Pawn)
                ((Pawn)piece).promotion(this);
            restartEnPassant(piece.isBlack());
        }
        public bool isEnPassantValidOnPiece(int x, int y)
        {
            if (board[x, y] is Pawn)
                if (((Pawn)board[x, y]).isEnPassantValid())
                    return true;
            return false;

        }
        public void updateBoard(Piece piece, int x, int y)  /// split
        {
            // 50 moves
            if (piece is Pawn || !this.isCellEmpty(x, y))
                turnsCountFor50MovesCount = 0;
            else
                turnsCountFor50MovesCount++;
            // en passant
            if (piece is Pawn)
            {
                if (Math.Abs(piece.getX() - x) == 2)
                    ((Pawn)piece).setEnPassant(true);
                else
                    if (piece.isBlack() && isEnPassantValidOnPiece(x + 1, y))
                    this.board[x + 1, y] = null;
                else if (!piece.isBlack() && isEnPassantValidOnPiece(x - 1, y))
                    this.board[x - 1, y] = null;
            }
            // castling 
            if (piece is King && Math.Abs(piece.getY() - y) == 2)
            {
                if (y == 2)
                    this.movePiece(board[x, 0], x, 3);
                else if (y == 6)
                    this.movePiece(board[x, 7], x, 5);
            }
            // checking if pieces mooved for castling, peons double move
            if (piece is Pawn || piece is Rook || piece is King)
                piece.movePiece();
            // keeping location of the king
            if (piece is King)
                updateKingLocation(piece.isBlack(), x, y);
            this.movePiece(piece, x, y);
        }
        public void restartEnPassant(bool blackTurn)
        {
            foreach (Piece piece in board)
            {
                if (piece is Pawn && piece.isBlack() == blackTurn)
                {
                    if (((Pawn)piece).isEnPassantValid())
                        ((Pawn)piece).setEnPassant(false);
                }
            }
        }
        public void updateKingLocation(bool black, int x, int y)
        {
            if (black)
            {
                this.blackKingX = x; this.blackKingY = y;
            }
            else
            {
                this.whiteKingX = x; this.whiteKingY = y;
            }
        }
        public bool isCellEmpty(int x, int y)
        {
            if (this.board[x, y] == null) return true;
            else return false;
        }
        public bool isEmptyPathY(Piece piece, int x, int y)
        {
            //moves in straight line in y only
            if (piece.getX() == x)
            {
                if (piece.getY() > y)
                    for (int i = piece.getY() - 1; i > y - 1; i--)
                    {
                        if (!(this.isCellEmpty(x, i)))
                            return false;
                    }
                else
                    for (int i = piece.getY() + 1; i < y + 1; i++)
                    {
                        if (!(this.isCellEmpty(x, i)))
                            return false;
                    }
                return true;
            }
            return false;
        }
        public bool isEmptyPathX(Piece piece, int x, int y)
        {
            //moves in straight line in x only
            if (piece.getY() == y)
            {
                if (piece.getX() > x)
                    for (int i = piece.getX() - 1; i > x - 1; i--)
                    {
                        if (!(this.isCellEmpty(i, y)))
                            return false;
                    }
                else
                    for (int i = piece.getX() + 1; i < x + 1; i++)
                    {
                        if (!(this.isCellEmpty(i, y)))
                            return false;
                    }
                return true;
            }
            return false;
        }
        public bool isDiagonalEmptyPath(Piece piece, int x, int y)
        {
            //moves diagonly
            int steps = Math.Abs(piece.getX() - x);
            if (piece.getX() < x)
            {
                if (piece.getY() < y)
                {
                    for (int i = 1; i < steps + 1; i++)
                    {
                        if (!(this.isCellEmpty(piece.getX() + i, piece.getY() + i)))
                            return false;
                    }
                    return true;
                }
                else   // p.x<x,p.y>y
                {
                    for (int i = 1; i < steps + 1; i++)
                    {
                        if (!(this.isCellEmpty(piece.getX() + i, piece.getY() - i)))
                            return false;
                    }
                    return true;
                }
            }
            else   // p.x>x
            {
                if (piece.getY() < y)
                {
                    for (int i = 1; i < steps + 1; i++)
                    {
                        if (!(this.isCellEmpty(piece.getX() - i, piece.getY() + i)))
                            return false;
                    }
                    return true;
                }
                else   // p.x>x,p.y>y
                {
                    for (int i = 1; i < steps + 1; i++)
                    {
                        if (!(this.isCellEmpty(piece.getX() - i, piece.getY() - i)))
                            return false;
                    }
                    return true;
                }
            }
        }
        public bool isCheckAfterMove(bool BlackTurn, Piece pieceMooving, int x, int y)
        {
            Piece savePiece = this.board[x, y];
            int savedX = pieceMooving.getX(), savedY = pieceMooving.getY();
            movePiece(pieceMooving, x, y);
            int kingX = BlackTurn ? blackKingX : whiteKingX, kingY = BlackTurn ? blackKingY : whiteKingY;
            foreach (Piece piece in this.board)
            {
                if (piece is null) ;
                else if (pieceMooving is King)
                {
                    kingX = x;
                    kingY = y;
                }
                if (piece is null) ;
                else if (piece.isBlack() == BlackTurn) ;
                else if (piece.isLegalMove(kingX, kingY, this))
                {
                    movePiece(pieceMooving, savedX, savedY);
                    this.board[x, y] = savePiece;
                    return false;
                }
            }
            movePiece(pieceMooving, savedX, savedY);
            this.board[x, y] = savePiece;
            return true;
        }
        public bool findThreatsOnKing(bool blackTurn)
        {
            int kingX = blackTurn ? blackKingX : whiteKingX, kingY = blackTurn ? blackKingY : whiteKingY, threatsCount = 0;
            Piece[] threats = new Piece[3];
            foreach (Piece piece in this.board)
            {
                if (piece is null) ;
                else if (piece.isBlack() == blackTurn) ;
                else if (piece.isLegalMove(kingX, kingY, this))
                {
                    threats[threatsCount] = piece;
                    threatsCount++;
                }
            }
            return threats.Length == 0;
        }
        public bool isStalemate(bool blackTurn)
        {
            foreach (Piece piece in this.board)
            {
                if (piece is null) ;
                else if (!piece.isBlack() == blackTurn) ;
                else for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (piece.isLegalMove(i, j, this) && this.isCheckAfterMove(blackTurn, piece, i, j))
                                return false;
                        }
                    }
            }
            return true;
        }
        public bool isWin(bool blackTurn)
        {
            if (findThreatsOnKing(blackTurn))
                return false;
            else
            {
                foreach (Piece piece in this.board)
                {
                    if (piece is null) ;
                    else if (piece.isBlack() != blackTurn) ;
                    else
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                if (piece.isLegalMove(i, j, this) && this.isCheckAfterMove(blackTurn, piece, i, j))
                                    return false;
                            }
                        }
                }
            }
            return true;
        }
        public bool isInsufficientMaterial()
        {
            Piece[] whatIsleft = new Piece[32];
            int count = 0;
            foreach (Piece piece in board)
            {
                if (piece != null)
                {
                    whatIsleft[count] = piece;
                    count++;
                }
            }
            if (count == 2)
                return true;
            else if (count == 3)
            {
                foreach (Piece piece in whatIsleft)
                {
                    if (piece is Knight)
                        return true;
                    else return false;
                }
            }
            return false;
        }
        public void updateBoardHistory(bool blackTurn)
        {
            // 3 repetition
            boardPos = "";
            if (!blackTurn)
                turnCount++;
            foreach (Piece pieceOnBoard in board)
            {
                if (pieceOnBoard is null)
                    boardPos += "00";
                else if (pieceOnBoard is Pawn)
                {

                    if (((Pawn)pieceOnBoard).isEnPassantValid())
                    {
                        if (pieceOnBoard.isBlack())
                            boardPos += "70";
                        else
                            boardPos += "71";
                    }
                    else if (pieceOnBoard.isBlack())
                        boardPos += "10";
                    else
                        boardPos += "11";
                }

                else if (pieceOnBoard is Rook)
                    if (pieceOnBoard.isBlack())
                        boardPos += "20";
                    else
                        boardPos += "21";
                else if (pieceOnBoard is Knight)
                    if (pieceOnBoard.isBlack())
                        boardPos += "30";
                    else
                        boardPos += "31";
                else if (pieceOnBoard is Bishop)
                    if (pieceOnBoard.isBlack())
                        boardPos += "40";
                    else
                        boardPos += "41";
                else if (pieceOnBoard is Queen)
                    if (pieceOnBoard.isBlack())
                        boardPos += "50";
                    else
                        boardPos += "51";
                else if (pieceOnBoard is King)
                    if (pieceOnBoard.isBlack())
                        boardPos += "60";
                    else
                        boardPos += "61";
            }
            (blackTurn ? blackBoardHistory : whiteBoardHistory)[turnCount] = boardPos;
        }
        public bool isThreefoldRepetitionCheck(bool blackTurn)
        {
            int count;
            string[] boardhistory = blackTurn ? blackBoardHistory : whiteBoardHistory;
            for (int i = 0; i < turnCount + 1; i++)
            {
                count = 0;
                for (int j = 0; j < turnCount + 1; j++)
                {
                    if (j == i) ;
                    else if (boardhistory[i] == boardhistory[j])
                        count++;
                    if (count == 2)
                        return true;
                }
            }
            return false;
        }
    }
    class Pawn : Piece
    {
        bool enPassantAble;
        public Pawn(int x, int y, bool black) : base(x, y, black) { enPassantAble = false; }
        public bool isEnPassantValid()
        {
            return enPassantAble;
        }
        public void setEnPassant(bool enPassantAble)
        {
            this.enPassantAble = enPassantAble;
        }
        public override bool isLegalMove(int x, int y, Board board)
        {
            if (board.isEmptyPathX(this, x, y) && this.getY() == y) // move 1 or 2: path is empty, didnt change y
            {
                if (!(this.isBlack()))
                {
                    if (!this.isMooved() && x == 3) //2 foreward: did not move,wants to move 2
                        return true;
                    else if (x - this.getX() == 1) //1 foreward: wants to move 1
                        return true;
                }
                else
                {
                    if (!this.isMooved() && x == 4) //2 foreward: did not move,wants to move 2
                        return true;
                    else if (x - this.getX() == -1) //1 foreward: wants to move 1
                        return true;
                }
            }
            else if (Math.Abs(y - this.getY()) == 1 && !(board.isCellEmpty(x, y)) && board.isPieceBlack(x, y) != this.isBlack()) //eat: moves only 1 in y, target location is occupied, piece is diffrent color from yours
            {
                if (x - this.getX() == -1 && this.isBlack()) //eats:wants to move 1,black
                    return true;
                else if (x - this.getX() == 1 && !(this.isBlack())) //eats:wants to move 1,white
                    return true;
            }

            else if (Math.Abs(y - this.getY()) == 1 && Math.Abs(x - this.getX()) == 1)//en passant: change y by 1, y is the same as peon that eligible to en passant, x changed by 1, en passant valid
            {
                if (x == 7 || x == 0)
                    return false;
                else if (this.isBlack() && board.isEnPassantValidOnPiece(x + 1, y))
                    return true;
                else if (!this.isBlack() && board.isEnPassantValidOnPiece(x - 1, y))
                    return true;
            }
            return false;
        }
        public void promotion(Board board) // mayby move to game, fix double appearing(one in ischeck, second on the movePiece
        {
            int x, y;
            bool black;
            string input;
            bool done = false;
            do
            {
                Console.WriteLine("how will the peon evolve? R for rook, B for bishop, N for knight, Q for queen and then enter");
                input = Console.ReadLine().Trim().ToUpper();
                if (input == null || input.Length != 1)
                    Console.WriteLine("wrong input please try again :(");
                else
                {
                    x = this.getX();
                    y = this.getY();
                    black = this.isBlack();
                    switch (input[0])
                    {
                        case 'R': board.getBoard()[x, y] = new Rook(x, y, black); done = true; break;
                        case 'B': board.getBoard()[x, y] = new Bishop(x, y, black); done = true; break;
                        case 'N': board.getBoard()[x, y] = new Knight(x, y, black); done = true; break;
                        case 'Q': board.getBoard()[x, y] = new Queen(x, y, black); done = true; break;
                        default: Console.WriteLine("wrong input please try again :("); break;
                    }
                }
            } while (!done);
        }
    }
    class Rook : Piece
    {
        public Rook(int x, int y, bool black) : base(x, y, black) { }
        public override bool isLegalMove(int x, int y, Board board)
        {
            if (y == this.getY()) //move in x: y havent changed
            {
                if (board.isEmptyPathX(this, x, y)) //move only
                    return true;
                else if (!(board.isCellEmpty(x, y)) && board.isPieceBlack(x, y) != this.isBlack()) //eat: location is occupied, opposite color
                {
                    if (this.getX() > x && board.isEmptyPathX(this, x + 1, y))
                        return true;
                    else if (this.getX() < x && board.isEmptyPathX(this, x - 1, y))
                        return true;
                }
            }
            else if (x == this.getX()) //move in y: x havent changed
            {
                if (board.isEmptyPathY(this, x, y)) //move only
                    return true;
                else if (!(board.isCellEmpty(x, y)) && board.isPieceBlack(x, y) != this.isBlack()) //eat: all path is empty, location is occuiped, opposite color
                {
                    if (this.getY() > y && board.isEmptyPathY(this, x, y + 1))
                        return true;
                    else if (this.getY() < y && board.isEmptyPathY(this, x, y - 1))
                        return true;
                }
            }
            return false;
        }
    }
    class Knight : Piece
    {
        public Knight(int x, int y, bool black) : base(x, y, black) { }
        public override bool isLegalMove(int x, int y, Board board)
        {
            if (Math.Abs(this.getX() - x) == 2 && Math.Abs(this.getY() - y) == 1 && (board.isCellEmpty(x, y) || board.isPieceBlack(x, y) != this.isBlack())) // move or eat: jump like a knight with 2 steps x 1 step y, board is empty or not the same color
                return true;
            else if (Math.Abs(this.getX() - x) == 1 && Math.Abs(this.getY() - y) == 2 && (board.isCellEmpty(x, y) || board.isPieceBlack(x, y) != this.isBlack()))
                return true;
            return false;
        }
    }
    class Bishop : Piece
    {
        public Bishop(int x, int y, bool black) : base(x, y, black) { }
        public override bool isLegalMove(int x, int y, Board board)
        {
            if (Math.Abs(this.getX() - x) != Math.Abs(this.getY() - y))
                return false;
            else if (board.isDiagonalEmptyPath(this, x, y)) //moves
                return true;
            else if (board.isCellEmpty(x, y))
                return false;
            else if (board.isPieceBlack(x, y) != this.isBlack()) //eats: path is clear, target piece is not yours
            {
                if (this.getX() > x)
                {
                    if (this.getY() > y && board.isDiagonalEmptyPath(this, x + 1, y + 1))
                        return true;
                    else if (this.getY() < y && board.isDiagonalEmptyPath(this, x + 1, y - 1))
                        return true;
                }
                else
                {
                    if (this.getY() > y && board.isDiagonalEmptyPath(this, x - 1, y + 1))
                        return true;
                    else if (this.getY() < y && board.isDiagonalEmptyPath(this, x - 1, y - 1))
                        return true;
                }
            }
            return false;
        }
    }
    class Queen : Piece
    {
        public Queen(int x, int y, bool black) : base(x, y, black) { }
        Bishop queenAsBishop;
        Rook queenAsRook;
        public override bool isLegalMove(int x, int y, Board board)
        {
            queenAsBishop = new Bishop(this.getX(), this.getY(), this.isBlack());
            queenAsRook = new Rook(this.getX(), this.getY(), this.isBlack());
            if (queenAsBishop.isLegalMove(x, y, board))
                return true;
            else if (queenAsRook.isLegalMove(x, y, board))
                return true;
            return false;
        }
    }
    class King : Piece
    {
        public King(int x, int y, bool black) : base(x, y, black) { }
        public override bool isLegalMove(int x, int y, Board board)
        {
            if (Math.Abs(this.getX() - x) <= 1 && Math.Abs(this.getY() - y) <= 1)
            {
                if (board.isCellEmpty(x, y)) //move
                    return true;
                else if (board.isPieceBlack(x, y) != this.isBlack()) //eat: only the other color
                    return true;
            }
            else if (this.getX() - x == 0 && Math.Abs(this.getY() - y) == 2) //castling: dosent move in x,exacly 2 in y, path is clear , rook hasnt mooved, king is not threatend in the step 
            {
                if (this.getY() > y && !this.isMooved() && board.getBoard()[this.getX(), 0] is Rook && !board.getBoard()[this.getX(), 0].isMooved() && this.isLegalMove(this.getX(), y + 1, board))
                {
                    if (!board.getBoard()[this.getX(), 0].isMooved() && board.getBoard()[this.getX(), 0].isLegalMove(this.getX(), 3, board))
                        return true;
                }
                else if (this.getY() < y && !this.isMooved() && board.getBoard()[this.getX(), 7] is Rook && !board.getBoard()[this.getX(), 7].isMooved() && this.isLegalMove(this.getX(), y - 1, board))
                {
                    if (!board.getBoard()[this.getX(), 7].isMooved() && board.getBoard()[this.getX(), 7].isLegalMove(this.getX(), 5, board))
                        return true;
                }
            }
            return false;
        }
    }
    class ChessGame
    {
        public void play()
        {
            int Xpiece, Ypiece, XLoc, YLoc;
            string input;
            bool blackTurn = false, turnOver = false, win = false, check = false;
            Board board = new Board();
            while (!win)
            {
                if (board.findThreatsOnKing(blackTurn))
                {
                    if (board.isWin(blackTurn))
                    {
                        Console.WriteLine("game is over, {0} won", blackTurn ? "white" : "black");
                        win = true;
                        turnOver = true;
                        board.printBoard();
                    }
                    else
                    {
                        check = true;
                        turnOver = false;
                    }
                }
                else if (board.isStalemate(blackTurn))
                {
                    Console.WriteLine("game is over, stalemate");
                    win = true;
                    turnOver = true;
                }
                else if (board.isInsufficientMaterial())
                {
                    Console.WriteLine("game is over, insufficient material");
                    board.printBoard();
                    win = true;
                    turnOver = true;
                }
                else if (board.getTurnsCountFor50MovesCount() == 50)
                {
                    win = true;
                    turnOver = true;
                    Console.WriteLine("50 moves without peon move or eating a piece, the game is a draw");
                }
                else if (board.isThreefoldRepetitionCheck(blackTurn))
                {
                    win = true;
                    turnOver = true;
                    Console.WriteLine("threefold repetition, the game is a draw");
                }
                else
                    turnOver = false;
                if (check)
                    Console.WriteLine("there is a check on {0} king", blackTurn ? "black" : "white");
                while (!turnOver)
                {
                    board.printBoard();
                    Console.WriteLine("");
                    Console.WriteLine("{0} player, please enter your next move (example A2A4), draw to ask draw or resign to resign", blackTurn ? "black" : "white");
                    input = Console.ReadLine().Trim().ToUpper();
                    if (checkInput(input)) // mayby addittional small funcs
                    {
                        if (input == "RESIGN")
                        {
                            win = true;
                            Console.WriteLine("{0} have resigned. game is over", blackTurn ? "black" : "white");
                            break;
                        }
                        if (input == "DRAW")
                        {
                            Console.WriteLine("do you accept, {0} player (Y for yes, anything else for no)", !blackTurn);
                            input = Console.ReadLine().Trim().ToUpper();
                            if (input == "Y")
                            {
                                win = true;
                                Console.WriteLine("its a draw. game is over");
                                break;
                            }
                            else
                                break;
                        }
                        Xpiece = int.Parse(input[1] + "") - 1;
                        Ypiece = input[0] - 65;
                        XLoc = int.Parse(input[3] + "") - 1;
                        YLoc = input[2] - 65; ;
                        if (board.isCellEmpty(Xpiece, Ypiece)) ;
                        else if (!board.isCheckAfterMove(blackTurn, board.getBoard()[Xpiece, Ypiece], XLoc, YLoc)) ;
                        else if (board.getBoard()[Xpiece, Ypiece].isLegalMove(XLoc, YLoc, board) && board.getBoard()[Xpiece, Ypiece].isBlack() == blackTurn)
                        {
                            check = false;
                            board.updateBoard(board.getBoard()[Xpiece, Ypiece], XLoc, YLoc);
                            board.updateBoardHistory(blackTurn);
                            turnOver = true;
                            blackTurn = !blackTurn;
                        }
                    }
                    if (!turnOver)
                        Console.WriteLine("wrong input please try again, its the {0} turn", blackTurn ? "black" : "white");
                    if (check)
                        Console.WriteLine("there is a check on {0} king", blackTurn ? "black" : "white");
                }
            }
            static bool checkInput(string input)
            {
                if (input == null) return false;
                else if (input == "RESIGN" || input == "DRAW") return true;
                else if (input.Length != 4) return false;
                switch (input[0])
                {
                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F': case 'G': case 'H': break;
                    default: return false;
                }
                switch (input[1])
                {
                    case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': break;
                    default: return false;
                }
                switch (input[2])
                {
                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F': case 'G': case 'H': break;
                    default: return false;
                }
                switch (input[3])
                {
                    case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': break;
                    default: return false;
                }
                return true;
            }
        }
    }
}

