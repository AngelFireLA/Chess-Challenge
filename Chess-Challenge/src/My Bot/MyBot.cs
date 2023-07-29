using System;
using ChessChallenge.API;
using System.Linq;
namespace ChessChallenge.Example{
public class MyBot : IChessBot
{
    Move emergencyMove;
    int BIG_VALUE = 300000;
    int[] p = {-21000, 100, 310, 330, 500, 1000, 10000 }; // Piece values: None, Pawn, Knight, Bishop, Rook, Queen, King
    ulong[] psts = {657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902};
    int[] piecePhase = {0, 0, 1, 1, 2, 4, 0};
    int globalDepth;
    

    public Move Think(Board board, Timer timer) {

        int timeLeft = timer.MillisecondsRemaining;
        
        if (timeLeft >= 50000) globalDepth = 6;
        else if (timeLeft >= 30000) globalDepth = 5;
        else if (timeLeft >= 7000) globalDepth = 4;
        else if (timeLeft >= 2000) globalDepth = 3;
        else if (timeLeft >= 500) globalDepth = 2;
        else if (timeLeft >= 100) globalDepth = 1;
        else return board.GetLegalMoves()[0];

        AlphaBeta(board, globalDepth, -BIG_VALUE, BIG_VALUE, board.IsWhiteToMove);
        return emergencyMove;
    }

    int AlphaBeta(Board board, int depth, int a, int b, bool white) {
        if (board.IsInCheckmate()) return -30000;
        if (board.IsRepeatedPosition() ||board.IsInsufficientMaterial() || board.FiftyMoveCounter > 99 || board.GetLegalMoves().Length <= 0) return 0;
        if (depth == 0) return Evaluate(board);


        var m = board.GetLegalMoves().OrderByDescending(x => p[(int)x.CapturePieceType] - p[(int)x.MovePieceType]).ToArray();
        int recordEval = -BIG_VALUE;
        foreach (var move in m)
        {
            board.MakeMove(move);
            var val = -AlphaBeta(board, depth - 1, -b, -a, !white);
            board.UndoMove(move);

            if (val > recordEval)
            {
                recordEval = val;
                if (depth == globalDepth)
                    emergencyMove = move;
            }

            a = Math.Max(a, recordEval);
            if (a >= b) break;
        }

        
        return recordEval;
    }


    public int Evaluate(Board board) {
        int mg = 0, eg = 0, phase = 0;

        foreach(bool stm in new[] {true, false}) {
            for(var pie = PieceType.Pawn; pie <= PieceType.King; pie++) {
                int piece = (int)pie, ind;
                ulong mask = board.GetPieceBitboard(pie, stm);
                while(mask != 0) {
                    phase += piecePhase[piece];
                    ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                    mg += getPstVal(ind) + p[piece];
                    eg += getPstVal(ind + 64) + p[piece];
                }
            }

            mg = -mg;
            eg = -eg;
        }
        

        return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
    }

    public int getPstVal(int psq) {
        return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    }
}
}

